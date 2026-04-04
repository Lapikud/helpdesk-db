"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { CategoryAssetsService } from "@/services/CategoryAssetsService";
import { CategoryService } from "@/services/CategoryService";
import { useRouter } from "next/navigation";

import Link from "next/link";
import { useContext, useEffect, useMemo, useState } from "react";
import { ICategoryAsset, ICategoryAssetWithNames } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import { AssetService } from "@/services/AssetService";

export default function CategoryAssets() {
	const { t: tCategoryAssets } = useTranslation("categoryAssets");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const categoryAssetsService: CategoryAssetsService = useMemo(
		() => new CategoryAssetsService(),
		[]
	);
	const categoryService: CategoryService = useMemo(
		() => new CategoryService(),
		[]
	);
	const assetService: AssetService = useMemo(
			() => new AssetService(),
			[]
		);
	if (setAccountInfo) {
		categoryAssetsService.injectSetAccountInfo(setAccountInfo);
		categoryService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
	}
	const router = useRouter();
	const [data, setData] = useState<ICategoryAssetWithNames[]>([]);
	const [hydrated, setHydrated] = useState(false);

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
			router.push("/");
			return;
		}

		const fetchData = async () => {
			try {
				const result = await categoryAssetsService.getAllAsync();
				if (result.errors) {
					console.log(result.errors);
					return;
				}
				const categoryAssetsWithNames = await Promise.all(
                    result.data!.map(async (categoryAsset) => {
                        const asset = await assetService.getAsync(categoryAsset.assetId);
                        const category = await categoryService.getAsync(categoryAsset.categoryId);
                        const assetName = asset.data?.assetName ?? categoryAsset.assetId;
                        const categoryName = category.data?.categoryName ?? categoryAsset.categoryId;
                        return { ...categoryAsset, assetName, categoryName };
                    })
                );
                setData(categoryAssetsWithNames);
			} catch (error) {
				console.error("Error fetching data:", error);
			}
		};

		fetchData();
	}, [hydrated, accountInfo, router, categoryAssetsService]);

	if (!hydrated) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-4">
				{tCategoryAssets("CategoryAssetsTitle")}
			</h1>
			{(isAdmin) && (
				<p className="mb-4">
					<Link
						href="/categoryAssets/create"
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
								{tCategoryAssets("Asset")}
							</th>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								{tCategoryAssets("Category")}
							</th>
							{/* <th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								{tCommon("Comment")}
							</th> */}
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								{tCommon("CreatedBy")}
							</th>
							{(isAdmin) && (
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
									{item.categoryName}
								</td>
								{/* <td className="px-6 py-4 border-b">
									{item.comment}
								</td> */}
								<td className="px-6 py-4 border-b">
									{item.createdBy}
								</td>
								{(isAdmin) && (
									<td className="px-6 py-4 border-b text-blue-600 space-x-2">
										<Link
											href={`/categoryAssets/edit/${item.id}`}
											className="hover:underline"
										>
											{tCommon("EditLink")}
										</Link>
										<span className="text-gray-400">|</span>
										<Link
											href={`/categoryAssets/details/${item.id}`}
											className="hover:underline"
										>
											{tCommon("DetailsLink")}
										</Link>
										<span className="text-gray-400">|</span>
										<Link
											href={`/categoryAssets/delete/${item.id}`}
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
