import { ErrorItem } from "./api.response";

export type Section<T> =
    | { status: 'ok'; data: T }
    | { status: 'error'; error: ErrorItem };

export function toSection<T>(
    value: T | null | undefined,
    errors: ErrorItem[],
    section: string,
): Section<T> {
    const sectionError= errors.filter(e => e.section === section);
    if (value != null && sectionError.length === 0) {
        return { status: 'ok', data: value };
    }
    return { status: 'error', error: sectionError[0] };
}