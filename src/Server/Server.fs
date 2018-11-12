open System.IO
open System.Threading.Tasks

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.Tasks.V2
open Giraffe
open Saturn
open Shared

open Giraffe.Serialization

let publicPath = Path.GetFullPath "../Client/public"
let port = 8085us

let api_handler =  router {
    post "/login" (fun next ctx ->
        task {
            let! login = ctx.BindJsonAsync<Login>()
            return! ctx.WriteJsonAsync login
        })
}

let webApp = router {
   forward "/api" api_handler
}

let configureSerialization (services:IServiceCollection) =
    let fableJsonSettings = Newtonsoft.Json.JsonSerializerSettings()
    fableJsonSettings.Converters.Add(Fable.JsonConverter())
    services.AddSingleton<IJsonSerializer>(NewtonsoftJsonSerializer fableJsonSettings)

let app = application {
    url ("http://0.0.0.0:" + port.ToString() + "/")
    use_router webApp
    memory_cache
    use_static publicPath
    service_config configureSerialization
    use_gzip
}

run app
