module MyIO

open System.Xml
open System.Xml.Serialization
open System.Xml.Linq
open System.IO.Ports
open System.Threading
open System.Reflection
open System.Runtime.Serialization
open System.Diagnostics

open NLog
open MyX
open my

type IMyPort =
    interface
        abstract member Write : byte[]->int->int->unit
        abstract member Read : byte[]->int->int->int
        abstract member BytesToRead : unit->int        
        abstract member What : unit -> string

        abstract member IsOpen : unit -> bool
        abstract member Open : unit -> unit
        abstract member Close : unit -> unit
    end

type ComPort () =
    inherit SerialPort()
    interface IMyPort with
        override this.Write buffer offset count     = 
            if base.IsOpen |> not then base.Open()
            base.Write( buffer, offset, count)
        override this.Read  buffer offset count     = 
            if base.IsOpen |> not then base.Open()
            base.Read( buffer, offset, count)
        override this.BytesToRead ()                = base.BytesToRead        
        override this.What()                        = base.PortName
        override this.IsOpen()                      = base.IsOpen
        override this.Open()                        = base.Open()
        override this.Close()                        = base.Close()

    member public this.ResetPortName portName = 
        if base.PortName <> portName then
            let was'opened = base.IsOpen            
            base.Close()
            base.PortName <- portName
            if was'opened then base.Open()

type Sets = 
    {         
        WriteDelay:int; 
        Timeout:int; 
        SilentTime:int; 
        RepeatCount:int
        EnableLog: bool; 
    }


type Bytes = System.Collections.Generic.IEnumerable<byte>
    
let private log = NLog.LogManager.GetCurrentClassLogger()

let show'exn (exn:System.Exception) (what: string option) =     
   (match what with
    | Some(what) -> sprintf "%s|Ошибка|%s" what (my.exn exn)
    | _ -> sprintf "Ошибка|%s" (my.exn exn)) |> log.Fatal


let write (txd:Bytes) (sets:Sets) (port:IMyPort) =
    if port.IsOpen() |> not then port.Open()
    if sets.EnableLog then 
        log.Info( "{0}|txd|{1}", port.What(), if txd |> Seq.isEmpty then "[]" else (bytesToStr txd) )
    let txd = Seq.toArray txd
    port.Write txd 0 txd.Length

type Resp = 
    NoResp | Error of string*string | RxD of Bytes | Canceled
    override this.ToString() = 
        match this with
            | NoResp -> "Нет ответа"            
            | Error(port,msg) -> port+"|"+msg
            | RxD(bytes) -> bytesToStr bytes            
            | Canceled -> "Прервано"
            

let sndrecv (txd:Bytes) (sets:Sets) (port:IMyPort) (ct:CancellationToken) = 

    let hasRxD () = port.BytesToRead() > 0
    let not'cncld () = not ct.IsCancellationRequested

    let rec dotry attemptN = 
        write txd sets port
        delayms ct sets.WriteDelay
        let timer = new Stopwatch()
        timer.Start()
        let rec recive' acc = 
            match acc with
            | RxD(readed) ->
                withCanceletionTime ct sets.Timeout hasRxD nop
                if not <| hasRxD() then RxD(readed) else 
                let countBytesToRead = port.BytesToRead()        
                let newPortion = Array.create countBytesToRead 0uy
                let readedCount = port.Read newPortion 0 countBytesToRead
                if readedCount<>countBytesToRead 
                then Error( port.What(), sprintf "считано %d байт из %d" readedCount countBytesToRead ) 
                else let readed = Seq.append readed newPortion 
                     delayms ct sets.SilentTime        
                     if port.BytesToRead()>0 && not'cncld() then recive' ( RxD(readed) ) else RxD(readed)
            | _ ->acc
        let answ = recive' ( RxD(Seq.empty) )
        timer.Stop()
        if ct.IsCancellationRequested then Resp.Canceled else  

        let s = sprintf "%s|%2.1g c" (port.What()) ( double( timer.Elapsed.TotalMilliseconds )/1000. ) 
        
        match answ with 
        | RxD(rxd) when rxd |> Seq.isEmpty ->   
            let noLastTry = attemptN<sets.RepeatCount
            if sets.EnableLog then 
                let s = sprintf "%s|нет ответа" s
                ( if noLastTry then s else sprintf "%s|попытка %d из %d" s (attemptN+1) (sets.RepeatCount+1) ) |> log.Error
            if noLastTry then dotry (attemptN+1) else NoResp                        
        | RxD(rxd) ->   if sets.EnableLog then sprintf "%s|%s" s (bytesToStr rxd) |> log.Info
                        RxD(rxd)
        | answ ->   if sets.EnableLog then sprintf "%s|%s" s ( answ.ToString() ) |> log.Warn
                    answ
//    dotry 0
    try 
        dotry 0
    with
    | :? System.UnauthorizedAccessException as exn ->
       Error( port.What(), sprintf "Невозможно использовать порт|%s" exn.Message ) 
    | _ as exn -> Error( port.What(),exn.Message )



module Modbus =

    type DeviceFailure = 
        | NoAnswer | LenMismatch of int | NonzeroCRC16 of uint16 | ErrorCode of byte 
        | AddyMismatch of byte
        | CmdCodeMismatch of byte 
        | CantConvertBCD of byte list
        | UnknownAnswer
        override this.ToString() = 
            match this with 
            | NoAnswer-> "Нет ответа"
            | LenMismatch(len) -> sprintf "Несоответствие дины ответа %d" len
            | NonzeroCRC16(crc16) -> sprintf "Ненулевая crc16 %x"  crc16
            | ErrorCode(b) -> sprintf "Прибор вернул код ошибки %d" b
            | AddyMismatch(addy) -> sprintf "Несовпадение адреса %d" addy
            | CmdCodeMismatch(cmd) -> sprintf "Несовпадение кода команды %d" cmd
            | UnknownAnswer -> "Неизвестный формат ответа"
            | CantConvertBCD(rx) -> sprintf "Недопустимая BCD-последовательность %s" (my.bytesToStr rx)
            

    type Answer = 
        | Error of string*string | DeviceFail of DeviceFailure | Data of byte list | Canceled
        override this.ToString() = 
            match this with                        
            | Data(bytes) -> bytesToStr bytes            
            | Error(port,s) -> port+"|"+s            
            | Canceled -> "Прервано"
            | DeviceFail(failure)-> failure.ToString()

    module Result =         
        type OkResult = 
            | OkRead3Float of double | OkWrite
            override this.ToString() = 
                match this with                        
                | OkRead3Float(v) -> v.ToString()
                | OkWrite -> "выполнена запись"
         
        type Rx = 
            | Ok of OkResult | DeviceFail of DeviceFailure | Error of Answer
            override this.ToString() = 
                match this with                        
                | Ok(v) -> v.ToString()
                | DeviceFail(fail) -> fail.ToString()
                | Error(error) -> error.ToString()

    let sndrecv addy cmdCode sets port ct txd =            
        let txd = seq { yield addy; yield cmdCode; yield! txd } |> addcrc16
        if addy=0uy then    
            write txd sets port
            delayms ct sets.WriteDelay 
            Data([])
        else
            match sndrecv txd sets port ct with
            | Resp.NoResp ->  DeviceFail NoAnswer
            | RxD(rxd) ->
                let err msg = Answer.Error( port.What(), sprintf "#%d|%s|%s|%s" addy ( port.What() ) msg (bytesToStr rxd) )                
                let len = Seq.length rxd 
                if len<4 then  DeviceFail( LenMismatch(len) ) else
                let crc16 = crc16 rxd
                if crc16>0us then DeviceFail( NonzeroCRC16 crc16 ) else    
                let rxd = rxd |> Seq.truncate (len-2) |> Seq.toList     
                match rxd with     
                | b::_ when b<>addy -> DeviceFail( AddyMismatch b)
                | _::b::errorCode::[] when b=(cmdCode|||0x80uy) -> DeviceFail( ErrorCode errorCode ) 
                | _::b::_ when b<>cmdCode -> DeviceFail( CmdCodeMismatch b )
                | _::_::rxd -> Data(rxd)
                |_ -> DeviceFail UnknownAnswer
            | Resp.Canceled -> Answer.Canceled
            | Resp.Error(what,s) -> Answer.Error(what,s)

    
        

    let write16 dt addy (deviceCommandCode:uint16) sets port ct =
        let ans = 
            let sendTxD cmdcode = 
                dt 
                |> Seq.append [| 0uy; 32uy; 0uy; 3uy; 6uy; byte(deviceCommandCode >>> 8);  byte deviceCommandCode |] 
                |>  sndrecv addy cmdcode sets port ct
            match sendTxD 0x16uy with
            | DeviceFail( NoAnswer ) -> sendTxD 0x10uy
            | els -> els
        match ans with
        | Data([]) when addy=0uy -> Result.Ok(Result.OkWrite)
        | Data(rxd) when rxd=0uy::32uy::0uy::3uy::[] -> Result.Ok(Result.OkWrite)
        | Data(_) -> Result.DeviceFail( UnknownAnswer )
        | DeviceFail(failure ) -> failure |> Result.DeviceFail
        | els ->  els |> Result.Error

    let write16val value = value |> floatBcd6 |> write16 

    let read3 addy (reg:uint16) regCount sets port ct =
        match [| byte(reg >>> 8); byte reg; byte(regCount >>> 8); byte regCount|] |>  
            sndrecv addy 3uy sets port ct with
        | Data(_::rxd) when rxd.Length=int(regCount)*2 -> Data(rxd)
        | Data(_) -> DeviceFail UnknownAnswer
        | els -> els

    let read3val addy (reg:uint16) sets port ct =
        let r = read3 addy reg 2 sets port ct
        match r with
        | Data( Bcd6Float v) -> v |> Result.OkRead3Float |> Result.Ok
        | Data(rx) -> rx |> DeviceFailure.CantConvertBCD |> Result.DeviceFail
        | DeviceFail(failure ) -> failure |> Result.DeviceFail
        | els -> els |> Result.Error
        //log.Info( sprintf "%s - прибор %d - [%d] - %s" (port.What()) addy reg (res.ToString()) )
        
        

