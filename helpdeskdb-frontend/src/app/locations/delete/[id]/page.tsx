"use client";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { LocationService } from "@/services/LocationService";
import { ILocation } from "@/types/domain/DomainTypes";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import { useTranslation } from "react-i18next";
import Spinner from "@/components/LoadingSpinner";

export default function LocationDelete({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tLocation } = useTranslation("location");
	const { t: tCommon } = useTranslation("common");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [errorMessage, setErrorMessage] = useState("");
	const [data, setData] = useState<ILocation>();
	const locationService: LocationService = useMemo(() => new LocationService(), []);

	if (setAccountInfo) {
		locationService.injectSetAccountInfo(setAccountInfo);
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
		} else {
			const fetchData = async () => {
				setData((await locationService.getAsync(id)).data!);
			};
			fetchData();
		}
	}, [hydrated, accountInfo, router, id, locationService, isAdmin]);

	const deleteConfirmed = async () => {
		try {
			const result = await locationService.deleteAsync(id);

			console.log("delete result", result);

			if (result.errors && result.errors.length > 0) {
				setErrorMessage(
					result.statusCode + " - " + result.errors.join(", ")
				);
				return;
			} else {
				setErrorMessage("");
				router.push("/locations");
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
					{tLocation("LocationSingular")}
				</h4>
				<hr className="border-gray-300" />
				{errorMessage.length > 0 && (
					<p className="text-red-600">{errorMessage}</p>
				)}
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tLocation("LocationName")}
						</dt>
						<dd className="w-2/3 text-gray-900">
							{data.locationName}
						</dd>
					</div>
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tLocation("ShelfNum")}
						</dt>
						<dd className="w-2/3 text-gray-900">
							{data.shelfNum}
						</dd>
					</div>
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tLocation("Column")}
						</dt>
						<dd className="w-2/3 text-gray-900">
							{data.column}
						</dd>
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
						href="/locations"
						className="text-blue-600 hover:underline font-medium"
					>
						{tCommon("BackToListLink")}
					</Link>
				</div>
			</div>
		</>
	);
}
