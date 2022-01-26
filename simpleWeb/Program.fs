open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe


[<CLIMutable>]
type Car =
    {
        Name   : string
        Make   : string
        Wheels : int
    }

let submitCar : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            // Binds a JSON payload to a Car object
            let! car = ctx.BindJsonAsync<Car>()

            // Sends the object back to the client
            return! Successful.OK car next ctx
        }

let webApp =
    choose [
        route "/ping" >=> text "pong"
        POST >=> route "/cars" >=> submitCar
    ]

type Startup() =
    member __.ConfigureServices (services : IServiceCollection) =
        // Register default Giraffe dependencies
        services.AddGiraffe() |> ignore

    member __.Configure (app : IApplicationBuilder)
                        (env : IHostEnvironment)
                        (loggerFactory : ILoggerFactory) =
        // Add Giraffe to the ASP.NET Core pipeline
        app.UseGiraffe webApp

[<EntryPoint>]
let main _ =
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .UseStartup<Startup>()
                    |> ignore)
        .Build()
        .Run()
    0