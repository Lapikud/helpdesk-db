"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { UserAssetsService } from "@/services/UserAssetsService";
import { UserService } from "@/services/UserService";
import { useRouter } from "next/navigation";

import Link from "next/link";
import { useContext, useEffect, useMemo, useState } from "react";
import { IUserAssetWithNames } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import { AssetService } from "@/services/AssetService";

export default function UserAssets() {
	const { t: tUserAssets } = useTranslation("userAssets");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const userAssetsService: UserAssetsService = useMemo(
		() => new UserAssetsService(),
		[]
	);
	const userService: UserService = useMemo(
		() => new UserService(),
		[]
	);
	const assetService: AssetService = useMemo(
			() => new AssetService(),
			[]
		);
	if (setAccountInfo) {
		userAssetsService.injectSetAccountInfo(setAccountInfo);
		userService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
	}
	const router = useRouter();
	const [data, setData] = useState<IUserAssetWithNames[]>([]);
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
				const result = await userAssetsService.getAllAsync();
				if (result.errors) {
					console.log(result.errors);
					return;
				}
				const userAssetsWithNames = await Promise.all(
                    result.data!.map(async (userAsset) => {
                        const asset = await assetService.getAsync(userAsset.assetId);
                        const user = await userService.getAsync(userAsset.userId);
                        const assetName = asset.data?.assetName ?? userAsset.assetId;
                        const userName = user.data?.userName ?? userAsset.userId;
                        return { ...userAsset, assetName, userName };
                    })
                );
                setData(userAssetsWithNames);
			} catch (error) {
				console.error("Error fetching data:", error);
			}
		};

		fetchData();
	}, [hydrated, accountInfo, router, userAssetsService]);

	if (!hydrated) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-4">
				{tUserAssets("UserAssetsMultiple")}
			</h1>
			{(isAdmin) && (
				<p className="mb-4">
					<Link
						href="/userAssets/create"
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
								{tUserAssets("Asset")}
							</th>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								{tUserAssets("User")}
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
									{item.userName}
								</td>
								{(isAdmin) && (
									<td className="px-6 py-4 border-b text-blue-600 space-x-2">
										<Link
											href={`/userAssets/edit/${item.id}`}
											className="hover:underline"
										>
											{tCommon("EditLink")}
										</Link>
										<span className="text-gray-400">|</span>
										<Link
											href={`/userAssets/details/${item.id}`}
											className="hover:underline"
										>
											{tCommon("DetailsLink")}
										</Link>
										<span className="text-gray-400">|</span>
										<Link
											href={`/userAssets/delete/${item.id}`}
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
