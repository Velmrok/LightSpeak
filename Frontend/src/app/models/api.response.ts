

export interface ApiResponse<TData>{
    data: TData;
    errors: ErrorItem[];
}

export interface ErrorResponse{
    code:string;
    details:string;
    errors:ErrorItem[];
}
export interface ErrorItem{
    section:string;
    code:string;
    details:string;
}