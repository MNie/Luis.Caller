namespace Luis

open MBrace.FsPickler.Json
open Hopac
open HttpFs.Client
open System
open Luis.Model
open Luis.Responses

type Caller(baseUrl: Uri, appId: Guid, versionId: string, apiKey: string) =
    let _apiKey = apiKey
    let jsonSerializer = FsPickler.CreateJsonSerializer(false, true)
    
    let _urlToApi = sprintf "%A/apps/%A/versions/%s" baseUrl appId versionId

    let createRequest url data methodType =
        Request.createUrl methodType url
        |> Request.setHeader (Custom ("Ocp-Apim-Subscription-Key", _apiKey))
        |> Request.autoDecompression DecompressionScheme.GZip
        |> Request.bodyStringEncoded data (System.Text.Encoding.UTF8)

    let createDelete url data = createRequest url data Delete
    let createGet url data = createRequest url data Get
    let createPost url data = createRequest url data Post
    let createPut url data = createRequest url data Put
    
    let call request =
        job {
            use! response = getResponse request
            let! result = Response.readBodyAsString response
          
            return result
        }
        |> Hopac.run

    //entities//
    let EntityOfType action entity url =
        let data = jsonSerializer.PickleToString(entity)

        action url data |> call

    let CreateEntityOfType entity typeOf = 
        let url = sprintf "%s/%s" _urlToApi typeOf
        EntityOfType createPost entity url

    let CreateEmptyEntity entity = CreateEntityOfType entity "entities"
    let CreateClosedListEntity entity = CreateEntityOfType entity "closedlists"
    let CreateHierarchicalEntity entity = CreateEntityOfType entity "hierarchicalentities"
    let CreateCompositeEntity entity = CreateEntityOfType entity "compositeentities"
    let CreatePrebuildEntity entity = CreateEntityOfType entity "prebuilts"
    
    let UpdateEntityOfType entity typeOf guid =
        let url = sprintf "%s/%s/%A" _urlToApi typeOf guid
        EntityOfType createPut entity url

    let UpdateEmptyEntity entity guid = UpdateEntityOfType entity "entities" guid
    let UpdateClosedListEntity entity guid = UpdateEntityOfType entity "closedlists" guid
    let UpdateHierarchicalEntity entity guid = UpdateEntityOfType entity "hierarchicalentities" guid
    let UpdateCompositeEntity entity guid = UpdateEntityOfType entity "compositeentities" guid

    // API //
    member this.CreateIntent(intentName: string) =
        let data = jsonSerializer.PickleToString(Intent(intentName))
        let url = sprintf "%s/intents" _urlToApi

        createPost url data |> call

    member this.CreateEntity(entity: Entity) =
        match entity with
        | EntityWithName e -> CreateEmptyEntity e
        | ClosedEntity e -> CreateClosedListEntity e
        | EntityWithHierarchy e -> CreateHierarchicalEntity e
        | EntityComposite e -> CreateCompositeEntity e
        | EntityPrebuild e -> CreatePrebuildEntity e

    member this.UpdateEntity(entity: Entity, entityId: Guid) =
        match entity with
        | EntityWithName e -> UpdateEmptyEntity e entityId
        | ClosedEntity e -> UpdateClosedListEntity e entityId
        | EntityWithHierarchy e -> UpdateHierarchicalEntity e entityId
        | EntityComposite e -> UpdateCompositeEntity e entityId

    member this.GetEntities(skip: int option, take: int option) =
        let toSkip = if skip.IsSome then skip.Value else 0
        let toTake = if take.IsSome then take.Value else 100
        let url = sprintf "%s/entities?skip=%A&take=%A" _urlToApi toSkip toTake

        createGet url "" |> call

    member this.BatchLabels(labels: Label []): LabelResponse =
        let url = sprintf "%s/examples" _urlToApi
        if Array.length labels <= 100 then 
            let data = jsonSerializer.PickleToString(labels)
            Success (createPost url data |> call)
        else Error (sprintf "there should be maximum 100 labels, but there are: %A" (Array.length labels))

    member this.Label(label) =
        let url = sprintf "%s/example" _urlToApi
        let data = jsonSerializer.PickleToString(label)
        createPost url data |> call

    member this.Train() =
        let url = sprintf "%s/train" _urlToApi
        createPost url "" |> call

module Caller =
    let create baseUrl appId versionId apiKey =
        Caller(baseUrl, appId, versionId, apiKey)