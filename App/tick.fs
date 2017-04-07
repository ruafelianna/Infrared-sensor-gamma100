[<AutoOpen>]
module MyTickModule


let ticks() = System.Environment.TickCount 

let oneTick = 1
let second = oneTick * 1000
let minute = second * 60
let hour = minute * 60

