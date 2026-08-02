"use client";

import { useTranslation } from "react-i18next";
import { useMemo } from "react";
import { useRefreshTokens, useUsers } from "@/hooks/queries/entityQueries";
import { IRefreshTokenWithUsername } from "@/types/domain/DomainTypes";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";

export default function RefreshTokens() {
	const { t: tRefreshToken } = useTranslation("refreshtoken");
	const { t: tCommon } = useTranslation("common");

	const { data: tokens, isError, error } = useRefreshTokens();
	const { data: users } = useUsers();

	const data: IRefreshTokenWithUsername[] = useMemo(() => {
		if (!tokens) return [];

		const userById = new Map((users ?? []).map((u) => [u.id, u.username]));

		return tokens.map((token) => ({
			...token,
			username: userById.get(token.userId) ?? token.userId,
		}));
	}, [tokens, users]);

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
			{isError && (
				<div className="mb-4 rounded-lg bg-red-100 border border-red-300 text-red-700 px-4 py-3">
					{tCommon("LoadFailed")}
					{error?.message ? `: ${error.message}` : ""}
				</div>
			)}
			<DataTable
				columns={columns}
				rows={rows}
				minWidth="min-w-[1100px]"
			/>
		</ListPageWrapper>
	);
}
