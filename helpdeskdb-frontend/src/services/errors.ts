import { IResultObject } from "@/types/IResultObject";

/**
 * Error thrown by `unwrap` when a service call fails.
 *
 * Services return `IResultObject` and never throw, but React Query decides
 * success/failure by whether the promise rejects — so failures have to be
 * converted into rejections. `statusCode` is carried through so the query
 * client can skip retries on 4xx (the 401 interceptor in BaseService already
 * owns token refresh; retrying would fight it).
 */
export class ApiError extends Error {
	constructor(
		message: string,
		public readonly statusCode: number,
		public readonly errors: string[],
	) {
		super(message);
		this.name = "ApiError";
	}
}

/** Best-effort human-readable message from an unknown thrown value. */
export function getErrorMessage(error: unknown): string {
	return error instanceof Error ? error.message : String(error);
}

/** Unwraps an `IResultObject` into its data, throwing `ApiError` on failure. */
export async function unwrap<T>(promise: Promise<IResultObject<T>>): Promise<T> {
	const result = await promise;

	if (result.errors?.length || (result.statusCode ?? 0) >= 400) {
		throw new ApiError(
			result.errors?.join(", ") || "Request failed",
			result.statusCode ?? 0,
			result.errors ?? [],
		);
	}

	return result.data as T;
}
