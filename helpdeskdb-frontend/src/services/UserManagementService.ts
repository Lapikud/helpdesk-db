import { IResultObject } from "@/types/IResultObject";
import { BaseService } from "./BaseService";
import { AxiosError } from "axios";

export class UserManagementService extends BaseService {
	constructor() {
		super();
	}

	// Add a role to a user
	async addRole(userId: string, roleId: string): Promise<IResultObject<null>> {
		try {
			const response = await this.axiosInstance.post<null>(
				`usermanagementapi/roleadd?userId=${userId}&roleId=${roleId}`
			);
			if (response.status < 300) {
				return { statusCode: response.status, data: null };
			}
			return {
				statusCode: response.status,
				errors: [(response.status.toString() + ' ' + response.statusText).trim()],
			};
		} catch (error) {
			return this.handleError(error);
		}
	}

	// Remove a role from a user
	async removeRole(userId: string, roleId: string): Promise<IResultObject<null>> {
		try {
			const response = await this.axiosInstance.delete<null>(
				`usermanagementapi/roleremove?userId=${userId}&roleId=${roleId}`
			);
			if (response.status < 300) {
				return { statusCode: response.status, data: null };
			}
			return {
				statusCode: response.status,
				errors: [(response.status.toString() + ' ' + response.statusText).trim()],
			};
		} catch (error) {
			return this.handleError(error);
		}
	}

	// Get password reset link for a user
	async getPasswordResetLink(userId: string): Promise<IResultObject<{ email: string; passwordResetLink: string }>> {
		try {
			const response = await this.axiosInstance.get<{ email: string; passwordResetLink: string }>(
				`usermanagementapi/passwordlink/${userId}`
			);
			if (response.status < 300) {
				return { statusCode: response.status, data: response.data };
			}
			return {
				statusCode: response.status,
				errors: [(response.status.toString() + ' ' + response.statusText).trim()],
			};
		} catch (error) {
			return this.handleError(error);
		}
	}

	private handleError<T>(error: unknown): IResultObject<T> {
		const axiosErr = error as AxiosError;
		console.log("UserManagementService error:", axiosErr.message);

		return {
			statusCode: axiosErr.status ?? 0,
			errors: [axiosErr.code ?? "UNKNOWN_ERROR"],
		};
	}
}
