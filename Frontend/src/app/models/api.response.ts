
interface ApiResponse<TData>{
    data: TData | null;
    errors: ErrorItem[];
}
interface ErrorResponse{
    code:String;
    details:String;
    errors:ErrorItem[];
}
interface ErrorItem{
    section:String;
    code:String;
    details:String;
}