"use client";

import { AccountContext } from "@/context/AccountContext";
import { AssetService } from "@/services/AssetService";
import { CategoryAssetsService } from "@/services/CategoryAssetsService";
import { CategoryService } from "@/services/CategoryService";
import { useRouter } from "next/navigation";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { ICategoryAssetWithNames } from "@/types/domain/DomainTypes";
import { useTranslation } from "react-i18next";
import Spinner from "@/components/LoadingSpinner";

export default function CategoryAssetsDetails({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tCategoryAsset } = useTranslation("categoryassets");
	const { t: tCommon } = useTranslation("common");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [data, setData] = useState<ICategoryAssetWithNames>();
	const [hydrated, setHydrated] = useState(false);
	const categoryAssetsService: CategoryAssetsService = useMemo(
		() => new CategoryAssetsService(),
		[]
	);
	const assetService: AssetService = useMemo(() => new AssetService(), []);
	const categoryService: CategoryService = useMemo(() => new CategoryService(), []);
	if (setAccountInfo) {
		categoryAssetsService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
		categoryService.injectSetAccountInfo(setAccountInfo);
	}

	const isAdmin = accountInfo?.roles?.includes("admins");

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;

		if (!accountInfo?.jwt) {
			router.push("/login");
		} else if (!isAdmin) {
			router.push("/");
			return;
		} else {
			const fetchData = async () => {
				try {
					const result = await categoryAssetsService.getAsync(id);
					if (result.errors) {
						console.log(result.errors);
						return;
					}
					const categoryAsset = result.data!;

					const assetResult = await assetService.getAsync(
						categoryAsset.assetId
					);
					let assetName;
					if (assetResult.errors) {
						console.log(assetResult.errors);
						return;
					} else {
						assetName = assetResult.data?.assetName!;
					}

					const categoryResult = await categoryService.getAsync(
						categoryAsset.categoryId
					);
					let categoryName;
					if (categoryResult.errors) {
						console.log(categoryResult.errors);
						return;
					} else {
						categoryName = categoryResult.data?.categoryName!;
					}

					setData({ ...categoryAsset, assetName, categoryName });
				} catch (error) {
					console.error("Error fetching data:", error);
				}
			};
			fetchData();
		}
	}, [hydrated, accountInfo, router, id, categoryAssetsService, assetService, categoryService, isAdmin]);

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
					{tCategoryAsset("CategoryAssetsSingular")}
				</h4>
				<hr className="border-gray-300" />
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tCategoryAsset("Asset")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.assetName}</dd>
					</div>
				</dl>
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tCategoryAsset("Category")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.categoryName}</dd>
					</div>
				</dl>
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tCommon("Comment")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.comment}</dd>
					</div>
				</dl>
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tCommon("CreatedBy")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.createdBy}</dd>
					</div>
				</dl>
			</div>
			<div>
				<Link
					href="/categoryAssets"
					className="text-blue-600 hover:underline font-medium"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
