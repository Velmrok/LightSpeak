
interface ApiResponse<TData>{
    data: TData | null;
    errors: ErrorItem[];
}
interface ErrorResponse{
    code:string;
    details:string;
    errors:ErrorItem[];
}
interface ErrorItem{
    section:string;
    code:string;
    details:string;
}