"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { RemovedAssetsService } from "@/services/RemovedAssetsService";
import { AssetService } from "@/services/AssetService";
import Link from "next/link";
import { useContext, useEffect, useMemo, useState } from "react";
import { IRemovedAssetWithAssetName } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import {
	ActionCell,
	EditButton,
	DeleteButton,
} from "@/components/TableActions";

export default function RemovedAssets() {
	const { t: tRemovedAssets } = useTranslation("removedassets");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const removedAssetsService = useMemo(() => new RemovedAssetsService(), []);
	const assetService = useMemo(() => new AssetService(), []);
	if (setAccountInfo) {
		removedAssetsService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
	}

	const [data, setData] = useState<IRemovedAssetWithAssetName[]>([]);
	const [hydrated, setHydrated] = useState(false);
	const isAdmin = accountInfo?.roles?.includes("admins");

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;
		const fetchData = async () => {
			try {
				const result = await removedAssetsService.getAllAsync();
				if (result.errors) return;
				const assetsWithNames = await Promise.all(
					result.data!.map(async (asset) => ({
						...asset,
						assetName: await assetService.getAssetNameById(
							asset.assetId,
						),
					})),
				);
				setData(assetsWithNames);
			} catch (error) {
				console.error("Error fetching data:", error);
			}
		};
		fetchData();
	}, [hydrated, assetService, removedAssetsService]);

	if (!hydrated) return <Spinner className="h-64" />;

	const columns = isAdmin
		? [
				tRemovedAssets("Asset"),
				tCommon("Comment"),
				tRemovedAssets("RemovedBy"),
				tCommon("Actions"),
			]
		: [
				tRemovedAssets("Asset"),
				tCommon("Comment"),
				tRemovedAssets("RemovedBy"),
			];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.assetName,
			item.comment || "-",
			item.removedBy,
			...(isAdmin
				? [
						<ActionCell key="actions">
							<EditButton
								href={`/removedAssets/edit/${item.id}`}
								label={tCommon("EditLink")}
							/>
							<DeleteButton
								href={`/removedAssets/delete/${item.id}`}
								label={tCommon("DeleteLink")}
							/>
						</ActionCell>,
					]
				: []),
		],
	}));

	return (
		<ListPageWrapper
			title={tRemovedAssets("RemovedAssetsTitle")}
			createButton={
				isAdmin && (
					<Link
						href="/removedAssets/create"
						className="bg-[#ff9800] hover:bg-[#f0941d] text-white font-medium px-6 py-3 rounded-full text-sm whitespace-nowrap transition-colors duration-150"
					>
						{tCommon("CreateNewLink")}
					</Link>
				)
			}
		>
			<DataTable columns={columns} rows={rows} minWidth="min-w-[500px]" />
		</ListPageWrapper>
	);
}
