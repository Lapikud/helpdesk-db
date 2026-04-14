"use client";

import { AccountContext } from "@/context/AccountContext";
import { CupboardService } from "@/services/CupboardService";
import { LocationInCupboardService } from "@/services/LocationInCupboardService";
import { LocationService } from "@/services/LocationService";
import { useRouter } from "next/navigation";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { ILocationInCupboardWithNames } from "@/types/domain/DomainTypes";
import { useTranslation } from "react-i18next";
import Spinner from "@/components/LoadingSpinner";

export default function LocationCupboardsDetails({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tLocationCupboard } = useTranslation("locationIncupboard");
	const { t: tCommon } = useTranslation("common");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [data, setData] = useState<ILocationInCupboardWithNames>();
	const [hydrated, setHydrated] = useState(false);
	const locationInCupboardService: LocationInCupboardService = useMemo(
		() => new LocationInCupboardService(),
		[]
	);
	const cupboardService: CupboardService = useMemo(() => new CupboardService(), []);
	const locationService: LocationService = useMemo(() => new LocationService(), []);
	if (setAccountInfo) {
		locationInCupboardService.injectSetAccountInfo(setAccountInfo);
		cupboardService.injectSetAccountInfo(setAccountInfo);
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
					const result = await locationInCupboardService.getAsync(id);
					if (result.errors) {
						console.log(result.errors);
						return;
					}
					const locationCupboard = result.data!;

					const cupboardResult = await cupboardService.getAsync(
						locationCupboard.cupboardId
					);
					let codeName;
					if (cupboardResult.errors) {
						console.log(cupboardResult.errors);
						return;
					} else {
						codeName = cupboardResult.data?.codeName!;
					}

					const locationResult = await locationService.getAsync(
						locationCupboard.locationId
					);
					let locationName;
					if (locationResult.errors) {
						console.log(locationResult.errors);
						return;
					} else {
						locationName = locationResult.data?.locationName!;
					}

					setData({ ...locationCupboard, codeName, locationName });
				} catch (error) {
					console.error("Error fetching data:", error);
				}
			};
			fetchData();
		}
	}, [hydrated, router, id, locationInCupboardService, cupboardService, locationService, isAdmin]);

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
					{tLocationCupboard("LocationInCupboardSingular")}
				</h4>
				<hr className="border-gray-300" />
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tLocationCupboard("Cupboard")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.codeName}</dd>
					</div>
				</dl>
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tLocationCupboard("Location")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.locationName}</dd>
					</div>
				</dl>

			</div>
			<div>
				<Link
					href="/locationsInCupboards"
					className="text-blue-600 hover:underline font-medium"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
