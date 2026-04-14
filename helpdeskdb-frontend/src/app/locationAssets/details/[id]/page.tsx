"use client";

import { AccountContext } from "@/context/AccountContext";
import { AssetService } from "@/services/AssetService";
import { LocationAssetsService } from "@/services/LocationAssetsService";
import { LocationService } from "@/services/LocationService";
import { useRouter } from "next/navigation";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { ILocationAssetWithNames } from "@/types/domain/DomainTypes";
import { useTranslation } from "react-i18next";
import Spinner from "@/components/LoadingSpinner";

export default function LocationAssetsDetails({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tLocationAsset } = useTranslation("locationassets");
	const { t: tCommon } = useTranslation("common");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [data, setData] = useState<ILocationAssetWithNames>();
	const [hydrated, setHydrated] = useState(false);
	const locationAssetsService: LocationAssetsService = useMemo(
		() => new LocationAssetsService(),
		[]
	);
	const assetService: AssetService = useMemo(() => new AssetService(), []);
	const locationService: LocationService = useMemo(() => new LocationService(), []);
	if (setAccountInfo) {
		locationAssetsService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
		locationService.injectSetAccountInfo(setAccountInfo);
	}

	const isAdmin = accountInfo?.roles?.includes("admins");

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;

  		if (!isAdmin) {
			router.push("/");
			return;
		} else {
			const fetchData = async () => {
				try {
					const result = await locationAssetsService.getAsync(id);
					if (result.errors) {
						console.log(result.errors);
						return;
					}
					const locationAsset = result.data!;

					const assetResult = await assetService.getAsync(
						locationAsset.assetId
					);
					let assetName;
					if (assetResult.errors) {
						console.log(assetResult.errors);
						return;
					} else {
						assetName = assetResult.data?.assetName!;
					}

					const locationResult = await locationService.getAsync(
						locationAsset.locationId
					);
					let locationName;
					if (locationResult.errors) {
						console.log(locationResult.errors);
						return;
					} else {
						locationName = locationResult.data?.locationName!;
					}

					setData({ ...locationAsset, assetName, locationName });
				} catch (error) {
					console.error("Error fetching data:", error);
				}
			};
			fetchData();
		}
	}, [hydrated, router, id, locationAssetsService, assetService, locationService, isAdmin]);

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
					{tLocationAsset("LocationAssetsSingular")}
				</h4>
				<hr className="border-gray-300" />
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tLocationAsset("Asset")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.assetName}</dd>
					</div>
				</dl>
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tLocationAsset("Location")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.locationName}</dd>
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
					href="/locationAssets"
					className="text-blue-600 hover:underline font-medium"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
