"use client";

import { AccountContext } from "@/context/AccountContext";
import { AssetService } from "@/services/AssetService";
import { UserAssetsService } from "@/services/UserAssetsService";
import { UserService } from "@/services/UserService";
import { useRouter } from "next/navigation";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { IUserAssetWithNames } from "@/types/domain/DomainTypes";
import { useTranslation } from "react-i18next";
import Spinner from "@/components/LoadingSpinner";

export default function UserAssetsDetails({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tUserAsset } = useTranslation("userassets");
	const { t: tCommon } = useTranslation("common");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [data, setData] = useState<IUserAssetWithNames>();
	const [hydrated, setHydrated] = useState(false);
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
			return;
		} else {
			const fetchData = async () => {
				try {
					const result = await userAssetsService.getAsync(id);
					if (result.errors) {
						console.log(result.errors);
						return;
					}
					const userAsset = result.data!;

					const assetResult = await assetService.getAsync(
						userAsset.assetId
					);
					let assetName;
					if (assetResult.errors) {
						console.log(assetResult.errors);
						return;
					} else {
						assetName = assetResult.data?.assetName!;
					}

					const userResult = await userService.getAsync(
						userAsset.userId
					);
					let userName;
					if (userResult.errors) {
						console.log(userResult.errors);
						return;
					} else {
						userName = userResult.data?.username!;
					}

					setData({ ...userAsset, assetName, userName });
				} catch (error) {
					console.error("Error fetching data:", error);
				}
			};
			fetchData();
		}
	}, [hydrated, accountInfo, router, id, userAssetsService, assetService, userService, isAdmin]);

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
					{tUserAsset("UserAssetSingular")}
				</h4>
				<hr className="border-gray-300" />
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tUserAsset("Asset")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.assetName}</dd>
					</div>
				</dl>
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tUserAsset("User")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.userName}</dd>
					</div>
				</dl>

			</div>
			<div>
				<Link
					href="/userAssets"
					className="text-blue-600 hover:underline font-medium"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
