"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { RefreshTokenService } from "@/services/RefreshTokenService";
import { UserService } from "@/services/UserService";
import { useRouter } from "next/navigation";

import { useCallback, useContext, useEffect, useMemo, useState } from "react";
import { IRefreshTokenWithUsername } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";

export default function RefreshTokens() {
	const { t: tRefreshToken } = useTranslation("refreshtoken");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const refreshTokenService: RefreshTokenService = useMemo(
		() => new RefreshTokenService(),
		[],
	);
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

	const fetchData = useCallback(async () => {
		const [tokensResult, usersResult] = await Promise.all([
			refreshTokenService.getAllAsync(),
			userService.getAllAsync(),
		]);

		if (
			tokensResult.errors ||
			!tokensResult.data ||
			usersResult.errors ||
			!usersResult.data
		) {
			return;
		}

		const userById = new Map(usersResult.data.map((u) => [u.id, u]));

		const withUsernames: IRefreshTokenWithUsername[] =
			tokensResult.data.map((token) => ({
				...token,
				username:
					userById.get(token.userId)?.username ?? token.userId,
			}));

		setData(withUsernames);
	}, [refreshTokenService, userService]);

	useEffect(() => {
		if (!hydrated) return;

		if (!isAdmin) {
			router.push("/");
			return;
		}

		fetchData();
	}, [hydrated, router, isAdmin, fetchData]);

	if (!hydrated) return <Spinner className="h-64" />;

	const columns = [
		tRefreshToken("User"),
		tRefreshToken("RefreshToken"),
		tRefreshToken("Expiration"),
		tRefreshToken("PreviousRefreshToken"),
		tRefreshToken("PreviousExpiration"),
	];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.username,
			item.refreshToken,
			new Date(item.expiration).toLocaleString(),
			item.previousRefreshToken || "-",
			item.previousExpiration
				? new Date(item.previousExpiration).toLocaleString()
				: "-",
		],
	}));

	return (
		<ListPageWrapper title={tRefreshToken("RefreshTokensTitle")}>
			<DataTable
				columns={columns}
				rows={rows}
				minWidth="min-w-[1100px]"
			/>
		</ListPageWrapper>
	);
}
