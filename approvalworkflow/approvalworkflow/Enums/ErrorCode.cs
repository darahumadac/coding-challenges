namespace approvalworkflow.Enums;

public static class ErrorEventId
{  
    public static EventId UserNotExists = new EventId((int)ErrorCode.UserNotExists, "USER_NOT_EXISTS");    
    public static EventId CategoryNotExists = new EventId((int)ErrorCode.CategoryNotExists, "CATEGORY_NOT_EXISTS");
    public static EventId UnexpectedErrorOnSave = new EventId((int)ErrorCode.UnexpectedErrorOnSave, "UNHANDLED_EXCEPTION_SAVE_RECORD");
    public static EventId UnauthorizedRequestAccess = new EventId((int)ErrorCode.UnauthorizedRequestAccess, "UNAUTHORIZED_REQUEST_ACCESS");

}

public enum ErrorCode
{
    UserNotExists = 10000,
    CategoryNotExists,
    UnexpectedErrorOnSave,
    UnauthorizedRequestAccess
}