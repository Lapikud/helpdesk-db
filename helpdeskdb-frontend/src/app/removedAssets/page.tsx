"use client";

import { AccountContext } from "@/context/AccountContext";
import { RemovedAssetsService } from "@/services/RemovedAssetsService";
import { AssetService } from "@/services/AssetService";
import { useRouter } from "next/navigation";

import Link from "next/link";
import { useContext, useEffect, useMemo, useState } from "react";
import { IRemovedAssetWithAssetName } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";

export default function RemovedAssets() {
	const { t: tRemovedAssets } = useTranslation("removedassets");
	const { t: tCommon } = useTranslation("common");
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [data, setData] = useState<IRemovedAssetWithAssetName[]>([]);
	const [hydrated, setHydrated] = useState(false);
	const removedAssetsService: RemovedAssetsService =
		useMemo(() => new RemovedAssetsService(), []);
	const assetService: AssetService = useMemo(() => new AssetService(), []);
	if (setAccountInfo) {
		removedAssetsService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
	}

	const isAdmin = accountInfo?.roles?.includes("admins");

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;

		if (!accountInfo?.jwt) {
			router.push("/login");
		}

		const fetchData = async () => {
			try {
				const result = await removedAssetsService.getAllAsync();

				if (result.errors) {
					console.log(result.errors);
					return;
				}
				// Fetch asset names for each removed asset
				const assetsWithNames = await Promise.all(
					result.data!.map(async (asset) => {
						const assetName = await assetService.getAssetNameById(
							asset.assetId
						);
						return { ...asset, assetName };
					})
				);
				setData(assetsWithNames);
			} catch (error) {
				console.error("Error fetching data:", error);
			}
		};

		fetchData();
	}, [hydrated, accountInfo, router, assetService, removedAssetsService]);

	if (!hydrated) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-4">
				{tRemovedAssets("RemovedAssetsTitle")}
			</h1>

			{isAdmin && (
				<p className="mb-4">
					<Link
						href="/removedAssets/create"
						className="inline-block bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 transition"
					>
						{tCommon("CreateNewLink")}
					</Link>
				</p>
			)}

			<div className="w-full max-w-7xl overflow-x-auto shadow rounded-lg">
				<table className="w-full table-auto bg-white border border-gray-200 text-left">
					<thead className="bg-gray-100">
						<tr>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								{tRemovedAssets("Asset")}
							</th>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								{tCommon("Comment")}
							</th>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								{tRemovedAssets("RemovedBy")}
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
									{item.assetName}
								</td>
								<td className="px-6 py-4 border-b">
									{item.comment}
								</td>
								<td className="px-6 py-4 border-b">
									{item.removedBy}
								</td>
								{isAdmin && (
									<td className="px-6 py-4 border-b text-blue-600 space-x-2">
										<Link
											href={`/removedAssets/edit/${item.id}`}
											className="hover:underline"
										>
											{tCommon("EditLink")}
										</Link>
										<span className="text-gray-400">|</span>
										<Link
											href={`/removedAssets/details/${item.id}`}
											className="hover:underline"
										>
											{tCommon("DetailsLink")}
										</Link>
										<span className="text-gray-400">|</span>
										<Link
											href={`/removedAssets/delete/${item.id}`}
											className="hover:underline"
										>
											{tCommon("DeleteLink")}
										</Link>
									</td>
								)}
							</tr>
						))}
					</tbody>
				</table>
			</div>
		</>
	);
}
