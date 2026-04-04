"use client";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { AssetService } from "@/services/AssetService";
import { CategoryAssetsService } from "@/services/CategoryAssetsService";
import { CategoryService } from "@/services/CategoryService";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";
import { ICategoryAssetWithNames } from "@/types/domain/DomainTypes";

export default function CategoryAssetsDelete({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tCategoryAssets } = useTranslation("categoryassets");
	const { t: tCommon } = useTranslation("common");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [errorMessage, setErrorMessage] = useState("");
	const [data, setData] = useState<ICategoryAssetWithNames>();
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
		}
		const fetchData = async () => {
			try {
				const result = await categoryAssetsService.getAsync(id);
				if (result.errors) {
					console.log(result.errors);
					return;
				}
				const categoryAssets = result.data!;

				const assetResult = (await assetService.getAsync(categoryAssets.assetId));
				let assetName;
				if (assetResult.errors) {
					console.log(assetResult.errors);
					return;
				} else {
					assetName = assetResult.data?.assetName!;
				}

				const categoryResult = (await categoryService.getAsync(categoryAssets.categoryId));
				let categoryName;
				if (categoryResult.errors) {
					console.log(categoryResult.errors);
					return;
				} else {
					categoryName = categoryResult.data?.categoryName!;
				}

				setData({...categoryAssets, assetName, categoryName});
			} catch (error) {
				console.error("Error fetching data:", error);
			}
		};
		fetchData();
	}, [hydrated, accountInfo, router, id, isAdmin, categoryAssetsService]);

	const deleteConfirmed = async () => {
		try {
			const result = await categoryAssetsService.deleteAsync(id);

			console.log("delete result", result);

			if (result.errors && result.errors.length > 0) {
				setErrorMessage(
					result.statusCode + " - " + result.errors.join(", ")
				);
				return;
			} else {
				setErrorMessage("");
				router.push("/categoryAssets");
			}
		} catch (error) {
			console.log("error: ", (error as Error).message);
			setErrorMessage((error as Error).message);
		}
	};

	if (!hydrated || !data) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-2">
				{tCommon("DeleteTitle")}
			</h1>

			<h3 className="text-lg text-gray-700 mb-4">
				{tCommon("DeleteConfirmQuestion")}
			</h3>
			<div className="bg-white p-6 rounded-lg shadow-md max-w-xl mx-auto space-y-4">
				<h4 className="text-xl font-medium text-gray-800">
					{tCategoryAssets("CategoryAssetsSingular")}
				</h4>
				<hr className="border-gray-300" />
				{errorMessage.length > 0 && (
					<p className="text-red-600">{errorMessage}</p>
				)}
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tCategoryAssets("Asset")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.assetName}</dd>
					</div>
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tCategoryAssets("Category")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.categoryName}</dd>
					</div>
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tCommon("Comment")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.comment}</dd>
					</div>
				</dl>
				<div className="mt-6 flex items-center space-x-4 justify-center">
					<button
						onClick={() => deleteConfirmed()}
						type="button"
						title="Delete"
						className="bg-red-600 hover:bg-red-700 text-white font-semibold py-2 px-4 rounded transition"
					>
						{tCommon("DeleteButton")}
					</button>

					<span className="text-gray-400">|</span>

					<Link
						href="/categoryAssets"
						className="text-blue-600 hover:underline font-medium"
					>
						{tCommon("BackToListLink")}
					</Link>
				</div>
			</div>
		</>
	);
}
