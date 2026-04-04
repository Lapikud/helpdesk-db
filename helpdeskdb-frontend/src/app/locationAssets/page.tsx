"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { LocationAssetsService } from "@/services/LocationAssetsService";
import { LocationService } from "@/services/LocationService";
import { useRouter } from "next/navigation";

import Link from "next/link";
import { useContext, useEffect, useMemo, useState } from "react";
import { ILocationAssetWithNames } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import { AssetService } from "@/services/AssetService";

export default function LocationAssets() {
	const { t: tLocationAssets } = useTranslation("locationAssets");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const locationAssetsService: LocationAssetsService = useMemo(
		() => new LocationAssetsService(),
		[]
	);
	const locationService: LocationService = useMemo(
		() => new LocationService(),
		[]
	);
	const assetService: AssetService = useMemo(
			() => new AssetService(),
			[]
		);
	if (setAccountInfo) {
		locationAssetsService.injectSetAccountInfo(setAccountInfo);
		locationService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
	}
	const router = useRouter();
	const [data, setData] = useState<ILocationAssetWithNames[]>([]);
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
				const result = await locationAssetsService.getAllAsync();
				if (result.errors) {
					console.log(result.errors);
					return;
				}
				for (let index = 0; index < result.data!.length; index++) {
					const element = result.data![index];
					console.log(element);
				}
				const locationAssetsWithNames = await Promise.all(
                    result.data!.map(async (locationAsset) => {
                        const asset = await assetService.getAsync(locationAsset.assetId);
                        const location = await locationService.getAsync(locationAsset.locationId);
                        const assetName = asset.data?.assetName ?? locationAsset.assetId;
                        const locationName = location.data?.locationName ?? locationAsset.locationId;
                        return { ...locationAsset, assetName, locationName };
                    })
                );
                setData(locationAssetsWithNames);
			} catch (error) {
				console.error("Error fetching data:", error);
			}
		};

		fetchData();
	}, [hydrated, accountInfo, router, isAdmin]);

	if (!hydrated) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-4">
				{tLocationAssets("LocationAssetsTitle")}
			</h1>
			{(isAdmin) && (
				<p className="mb-4">
					<Link
						href="/locationAssets/create"
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
								{tLocationAssets("Asset")}
							</th>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								{tLocationAssets("Location")}
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
									{item.locationName}
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
											href={`/locationAssets/edit/${item.id}`}
											className="hover:underline"
										>
											{tCommon("EditLink")}
										</Link>
										<span className="text-gray-400">|</span>
										<Link
											href={`/locationAssets/details/${item.id}`}
											className="hover:underline"
										>
											{tCommon("DetailsLink")}
										</Link>
										<span className="text-gray-400">|</span>
										<Link
											href={`/locationAssets/delete/${item.id}`}
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
