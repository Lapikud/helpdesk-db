"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { RefreshTokenService } from "@/services/RefreshTokenService";
import { useRouter } from "next/navigation";

import Link from "next/link";
import { useContext, useEffect, useMemo, useState } from "react";
import { IRefreshTokenWithUsername } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import { UserService } from "@/services/UserService";

export default function RefreshTokens() {
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const refreshTokenService: RefreshTokenService = useMemo(() => new RefreshTokenService(), []);
	const userService: UserService = useMemo(() => new UserService(), []);
	if (setAccountInfo) {
		refreshTokenService.injectSetAccountInfo(setAccountInfo);
		userService.injectSetAccountInfo(setAccountInfo);
	}
	const router = useRouter();
	const [data, setData] = useState<IRefreshTokenWithUsername[]>([]);
	const [hydrated, setHydrated] = useState(false);

	const isAdmin = accountInfo?.roles?.includes("admins");

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;

		if (!isAdmin) {
			router.push("/");
			return;
		}

		const fetchData = async () => {
			try {
				const result = await refreshTokenService.getAllAsync();
				if (result.errors) {
					console.log(result.errors);
					return;
				}
				const usersResult = await userService.getAllAsync();
				if (usersResult.errors) {
					console.log(usersResult.errors);
					return;
				}

				const refreshTokensWithUsername = await Promise.all(
                    result.data!.map(async (refreshToken) => {
                        const user = await userService.getAsync(refreshToken.userId);
                        const username = user.data?.username ?? refreshToken.userId;
                        return { ...refreshToken, username };
                    })
                );
                setData(refreshTokensWithUsername);
			} catch (error) {
				console.error("Error fetching data:", error);
			}
		};

		fetchData();
	}, [hydrated, router, refreshTokenService, userService, isAdmin]);

	if (!hydrated) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-4">RefreshTokens</h1>

			<div className="w-full max-w-7xl overflow-x-auto shadow rounded-lg">
				<table className="w-full table-auto bg-white border border-gray-200 text-left">
					<thead className="bg-gray-100">
						<tr>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								User
							</th>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								RefreshToken
							</th>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								Expiration
							</th>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								PreviousRefreshToken
							</th>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								PreviousExpiration
							</th>
							{isAdmin && (
								<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
									{tCommon("Actions")}
								</th>
							)}
						</tr>
					</thead>
					<tbody>
						{data.map((item) => (
							<tr key={item.id} className="hover:bg-gray-50">
								<td className="px-6 py-4 border-b">
									{item.username}
								</td>
								<td className="px-6 py-4 border-b">
									{item.refreshToken}
								</td>
								<td className="px-6 py-4 border-b">
									{new Date(
										item.expiration,
									).toLocaleString()}
								</td>
								<td className="px-6 py-4 border-b">
									{item.previousRefreshToken}
								</td>
								<td className="px-6 py-4 border-b">
									{new Date(
										item.previousExpiration,
									).toLocaleString()}
								</td>
								<td className="px-6 py-4 border-b text-blue-600 space-x-2">
									<Link
										href={`/refreshTokens/details/${item.id}`}
										className="hover:underline"
									>
										{tCommon("DetailsLink")}
									</Link>
								</td>
							</tr>
						))}
					</tbody>
				</table>
			</div>
		</>
	);
}
