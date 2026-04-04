"use client";

import { AccountContext } from "@/context/AccountContext";
import { UserService } from "@/services/UserService";
import { useRouter } from "next/navigation";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { IRefreshTokenWithUsername } from "@/types/domain/DomainTypes";
import { RefreshTokenService } from "@/services/RefreshTokenService";
import { useTranslation } from "react-i18next";
import Spinner from "@/components/LoadingSpinner";

export default function RefreshTokenDetails({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tCommon } = useTranslation("common");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [data, setData] = useState<IRefreshTokenWithUsername>();
	const [hydrated, setHydrated] = useState(false);

	const userService: UserService = useMemo(
		() => new UserService(),
		[]
	);
	const refreshTokenService: RefreshTokenService = useMemo(
		() => new RefreshTokenService(),
		[]
	);

	if (setAccountInfo) {
		refreshTokenService.injectSetAccountInfo(setAccountInfo);
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
		}

		const fetchData = async () => {
			try {
				const result = await refreshTokenService.getAsync(id);
				if (result.errors) {
					console.log(result.errors);
					return;
				}
				const userResult = await userService.getAsync(result.data?.userId!);
				if (userResult.errors) {
					console.log(userResult.errors);
					return;
				}
				const username = userResult.data?.userName!;
				const refreshTokenWithUsername: IRefreshTokenWithUsername = {...result.data!, username }

				setData(refreshTokenWithUsername);
			} catch (error) {
				console.error("Error fetching data:", error);
			}
		};
		fetchData();
	}, [
		hydrated,
		accountInfo,
		router,
		id,
		userService,
		refreshTokenService,
		isAdmin,
	]);

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
					RefreshToken
				</h4>
				<hr className="border-gray-300" />
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							Username
						</dt>
						<dd className="w-2/3 text-gray-900">
							{data.username}
						</dd>
					</div>
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							RefreshToken
						</dt>
						<dd className="w-2/3 text-gray-900">
							{data.refreshToken}
						</dd>
					</div>
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							Expiration
						</dt>
						<dd className="w-2/3 text-gray-900">
							{data.expiration.slice(0, 10)} {data.expiration.slice(11, 19)}
						</dd>
					</div>
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							Previous RefreshToken
						</dt>
						<dd className="w-2/3 text-gray-900">
							{data.previousRefreshToken}
						</dd>
					</div>
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							Previous expiration
						</dt>
						<dd className="w-2/3 text-gray-900">
							{data.previousExpiration.slice(0, 10)} {data.previousExpiration.slice(11, 19)}
						</dd>
					</div>
				</dl>
			</div>
			<div>
				<Link
					href="/refreshTokens"
					className="text-blue-600 hover:underline font-medium"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
