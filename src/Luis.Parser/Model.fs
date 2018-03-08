namespace Luis.Parser.DateTimeV2

open System
open Newtonsoft.Json

exception ParseError of string
type YearValue = YearValue of int
type PeriodType = Year | Month | Week | Day
type TimeType = Duration | DateRange

type Period = 
    {
        [<JsonProperty("interval")>]
        Interval: int
        [<JsonProperty("type")>]
        Type: PeriodType
    }
type Range = Range of DateTime * DateTime * Period

type TimexType = 
    | Year of YearValue
    | Period of Period
    | Range of Range

type DateRange = 
    {
        [<JsonProperty("timex")>]
        Timex: TimexType
        [<JsonProperty("type")>]
        Type: TimeType
        [<JsonProperty("start")>]
        Start: DateTime
        [<JsonProperty("end")>]
        End: DateTime
    }

type PeriodConverter() =
    inherit JsonConverter()

    override this.WriteJson(writer: JsonWriter, value: obj, serializer: JsonSerializer) =
        writer.WriteValue(sprintf "%A" value)

    override this.ReadJson(reader: JsonReader, objectType: Type, existingValue: obj, serializer: JsonSerializer): obj =
        let timex = downcast reader.Value: string
        let penulimate = timex.Length - 2
        let interval = timex.Substring(1, penulimate) |> int
        let t = timex |> Seq.last |> fun x -> 
                match x with
                | 'D' -> PeriodType.Day
                | 'W' -> PeriodType.Week
                | 'M' -> PeriodType.Month
                | 'Y' -> PeriodType.Year
                | _ -> raise (ParseError(sprintf "%c period type not found" x))
        {Interval = interval; Type = t} :> obj

    override this.CanConvert(objType: Type): bool = 
        typedefof<Period> = objType

type TimeTypeConverter() =
    inherit JsonConverter()

    override this.WriteJson(writer: JsonWriter, value: obj, serializer: JsonSerializer) =
        writer.WriteValue(sprintf "%A" value)

    override this.ReadJson(reader: JsonReader, objectType: Type, existingValue: obj, serializer: JsonSerializer): obj =
        (downcast reader.Value: string)
        |> fun x ->
            match x with
            | "duration" -> TimeType.Duration
            | "daterange" -> TimeType.DateRange
            | _ -> raise (ParseError(sprintf "%s time type not found" x))
        :> obj

    override this.CanConvert(objType: Type): bool =
        typedefof<Period> = objType

type Duration = 
    {
        [<JsonProperty("timex")>]
        [<JsonConverter(typeof<PeriodConverter>)>]
        Timex: Period
        [<JsonProperty("type")>]
        [<JsonConverter(typeof<TimeTypeConverter>)>]
        Type: TimeType
        [<JsonProperty("value")>]
        Value: int
    }
