namespace Omicx.QA.Elasticsearch.Enums;

public enum Operator
{
    Match,          // Text search
    MultiMatch,     // Tìm 1 value trên nhiều field, danh sách field cách nhau bằng dấu ","
    Eq,             // Equal (=)
    Ne,             // Not Equal (!=)
    Gt,             // Greater than (>)
    Gte,            // Greater than or Equal (>=)
    Lt,             // Less than (<)
    Lte,            // Less than or Equal (<=)
    In,             // IN: Tìm nhiều value trên 1 field
    Nin,            // NOT IN
    Between,
    Outside,
    Contain,
    Null,
    NotNull,
}