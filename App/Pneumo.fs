module Pneumo

open System
open System.Drawing

open MyX
open MIL82Gui
open Data

let private log = NLog.LogManager.GetCurrentClassLogger()
let private form = MIL82MainForm.form

type Clapan = 
    | Off | K1 | K2 | K3 | K4 | K5 | K5_b | K6_b | K7_b | K8_b

    member this.What = 
        match this with
        | Off -> "Отключить газ"    
        | _ -> sprintf "Включить газ %d" this.Code

    member this.WhatDo = 
        match this with
        | Off -> "Отключите газ"
        | _ -> sprintf "Подайте газ %d" this.Code

    member this.WhatBlow = 
        match this with
        | Off -> "Отключить газ"
        | _ -> sprintf "Продуть газ %d" this.Code
        

    member this.WhatPgs = sprintf "ПГС-ГСО №%d" this.Code
    static member Li = my.unionCases<Clapan>
    static member CheckNum num = Diagnostics.Debug.Assert( num>0 && num<Clapan.Li.Length )
    member public this.Code =         
        Clapan.Li
        |> List.mapi( fun n case ->  n,case) 
        |> List.find( fun (n,case) -> case=this )
        |> fst |> byte
    static member ByNum num = 
        Clapan.CheckNum num
        Clapan.Li 
        |> List.mapi( fun n case ->  n,case) 
        |> List.find( fun (n,case) -> n=num )
        |> snd   

type private Status = Error of string | Ok of Clapan

type private Modbus = MyIO.Modbus.Answer

let pgs = function 
    | K1 -> Dev.sets().ПГС1 
    | K2 -> Dev.sets().ПГС2 
    | K3 -> Dev.sets().ПГС3 
    | K4 -> Dev.sets().ПГС4
    | K5 -> Dev.sets().ПГС5
    | _ -> failwith "Egc! Pneumo.pgs"

let pgsconc() = [|Clapan.K1;Clapan.K2;Clapan.K3|] |> Array.map pgs