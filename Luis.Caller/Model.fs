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
