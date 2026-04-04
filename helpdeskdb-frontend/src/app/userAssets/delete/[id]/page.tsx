"use client";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { AssetService } from "@/services/AssetService";
import { UserAssetsService } from "@/services/UserAssetsService";
import { UserService } from "@/services/UserService";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";
import { IUserAssetWithNames } from "@/types/domain/DomainTypes";

export default function UserAssetsDelete({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tUserAssets } = useTranslation("userassets");
	const { t: tCommon } = useTranslation("common");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [errorMessage, setErrorMessage] = useState("");
	const [data, setData] = useState<IUserAssetWithNames>();
	const userAssetsService: UserAssetsService = useMemo(
		() => new UserAssetsService(),
		[]
	);
	const assetService: AssetService = useMemo(() => new AssetService(), []);
	const userService: UserService = useMemo(() => new UserService(), []);
	if (setAccountInfo) {
		userAssetsService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
		userService.injectSetAccountInfo(setAccountInfo);
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
				const result = await userAssetsService.getAsync(id);
				if (result.errors) {
					console.log(result.errors);
					return;
				}
				const userAssets = result.data!;

				const assetResult = (await assetService.getAsync(userAssets.assetId));
				let assetName;
				if (assetResult.errors) {
					console.log(assetResult.errors);
					return;
				} else {
					assetName = assetResult.data?.assetName!;
				}

				const userResult = (await userService.getAsync(userAssets.userId));
				let userName;
				if (userResult.errors) {
					console.log(userResult.errors);
					return;
				} else {
					userName = userResult.data?.userName!;
				}

				setData({...userAssets, assetName, userName});
			} catch (error) {
				console.error("Error fetching data:", error);
			}
		};
		fetchData();
	}, [hydrated, accountInfo, router, id, isAdmin, userAssetsService]);

	const deleteConfirmed = async () => {
		try {
			const result = await userAssetsService.deleteAsync(id);

			console.log("delete result", result);

			if (result.errors && result.errors.length > 0) {
				setErrorMessage(
					result.statusCode + " - " + result.errors.join(", ")
				);
				return;
			} else {
				setErrorMessage("");
				router.push("/userAssets");
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
					{tUserAssets("UserAssetSingular")}
				</h4>
				<hr className="border-gray-300" />
				{errorMessage.length > 0 && (
					<p className="text-red-600">{errorMessage}</p>
				)}
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tUserAssets("Asset")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.assetName}</dd>
					</div>
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tUserAssets("User")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.userName}</dd>
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
						href="/userAssets"
						className="text-blue-600 hover:underline font-medium"
					>
						{tCommon("BackToListLink")}
					</Link>
				</div>
			</div>
		</>
	);
}
