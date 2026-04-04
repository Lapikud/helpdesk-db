"use client";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { RemovedAssetsService } from "@/services/RemovedAssetsService";
import { AssetService } from "@/services/AssetService";
import { IRemovedAssetWithAssetName } from "@/types/domain/DomainTypes";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";


export default function RemovedAssetDetailsClient({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tRemovedAssets } = useTranslation("removedassets");
	const { t: tCommon } = useTranslation("common");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [data, setData] = useState<IRemovedAssetWithAssetName>();
	const [hydrated, setHydrated] = useState(false);

	const removedAssetsService: RemovedAssetsService = useMemo(
		() => new RemovedAssetsService(),
		[]
	);
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

		if (!isAdmin) {
			router.push("/removedAssets");
			return;
		}

		const fetchData = async () => {
			try {
				const result = await removedAssetsService.getAsync(id);
				if (result.errors) {
					console.log(result.errors);
					return;
				}
				const removedAsset = result.data!;
				const assetName = await assetService.getAssetNameById(
					removedAsset.assetId
				);

				setData({ ...removedAsset, assetName });
			} catch (error) {
				console.error("Error fetching data:", error);
			}
		};

		fetchData();
	}, [
		hydrated,
		accountInfo,
		router,
		id,
		assetService,
		isAdmin,
		removedAssetsService,
	]);

	if (!hydrated || !data) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-2">
				{tCommon("DetailsTitle")}
			</h1>

			<div className="bg-white p-6 rounded-lg shadow-md max-w-xl mx-auto space-y-4">
				<h4 className="text-xl font-medium text-gray-800">
					{tRemovedAssets("RemovedAssetsSingular")}
				</h4>
				<hr className="border-gray-300" />
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tRemovedAssets("Asset")}
						</dt>
						<dd className="w-2/3 text-gray-900">
							{data.assetName}
						</dd>
					</div>
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tCommon("Comment")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.comment}</dd>
					</div>
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tRemovedAssets("RemovedBy")}
						</dt>
						<dd className="w-2/3 text-gray-900">
							{data.removedBy}
						</dd>
					</div>
				</dl>
			</div>
			<div>
				<Link
					href={`/removedAssets/edit/${data.id}`}
					className="text-blue-600 hover:underline font-medium"
				>
					{tCommon("EditLink")}
				</Link>
				<span className="text-gray-400"> | </span>
				<Link
					href="/removedAssets"
					className="text-blue-600 hover:underline font-medium"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
