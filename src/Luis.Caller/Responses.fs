namespace Luis.Responses

type LabelResponse =
    | Success of string
    | Error of string