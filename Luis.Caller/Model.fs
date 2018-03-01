namespace Luis.Model

type internal CollectionWithName = 
    struct
        val name: string
        new(n: string) = {name = n}
    end

type ClosedList = 
    {
        canonicalForm: string
        list: string[]
    }

type ClosedListEntity = 
    {
        name: string
        sublists: ClosedList[]
    }

type HierarchicalEntity =
    {
        name: string
        children: string[]
    }
type CompositeEntity =
    {
        name: string
        children: string[]
    }

type EmptyEntity = EmptyEntity of string
type internal Intent = CollectionWithName

module EmptyEntity =
    let create name =
        EmptyEntity(name)

type Entity =
    | EntityWithName of EmptyEntity
    | ClosedEntity of ClosedListEntity
    | EntityWithHierarchy of HierarchicalEntity
    | EntityComposite of CompositeEntity
    | EntityPrebuild of string[]

type Label =
    {
        text: string
        intentName: string
        entityLabels: EntityLabel []
    }
and EntityLabel =
    {
        entityName: string
        startCharIndex: int
        endCharIndex: int
    }

type BingSpellCheck =
    {
        apiKey: string
    }

[<StructuredFormatDisplay("{StructuredFormatDisplay}")>]
type Predict =
    {
        query: string
        verbose: bool
        spellCheck: BingSpellCheck Option
        staging: bool
        log: bool
    }
    member this.WithoutQuery() =
        let s = 
            match this.spellCheck with
            | Some sc -> sprintf "spellCheck=%s" sc.apiKey
            | _ -> ""

        sprintf "verbose=%A&staging=%A&log=%A%s" this.verbose this.staging this.log s

    member this.StructuredFormatDisplay =
        sprintf "q=%s&%s" this.query (this.WithoutQuery())

    

module Label =
    let create text name entities =
        {text = text; intentName = name; entityLabels = entities}

module EntityLabel =
    let create name start ``end`` =
        {entityName = name; startCharIndex = start; endCharIndex = ``end``}

module HierarchicalEntity =
    let create name children =
        { name = name; children = children }

module ClosedListEntity =
    let create name list =
        { name = name; sublists = list }

module ClosedList =
    let create name values = 
        { canonicalForm = name; list = values }

module CompositeEntity =
    let create name children =
        { name = name; children = children }

module Predict =
    let create query verbose spell staging log =
        { query = query; verbose = verbose; spellCheck = spell; staging = staging; log = log}

    let createWithVerbose query spell staging log = 
        create query true spell staging log

    let createWithVerboseAndLog query spell staging = 
        create query true spell staging true

    let createWithVerboseAndLogAndStage query spell = 
        create query true spell true true

    let createWithLog query verbose spell staging = 
        create query verbose spell staging true

    let createWithStage query verbose spell log = 
        create query verbose spell true log

    let createWithoutSpell query verbose staging log = 
        create query verbose None staging log

    let createWithoutSpellButWithVerbose query staging log = 
        create query true None staging log

    let createWithoutSpellButWithVerboseAndLog query staging = 
        create query true None staging true

    let createWithoutSpellButWithVerboseAndLogAndStage query = 
        create query true None true true
