"use client";

import { AccountContext } from "@/context/AccountContext";
import { RoleService } from "@/services/RoleService";
import { useRouter } from "next/navigation";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { IRole } from "@/types/domain/DomainTypes";
import { useTranslation } from "react-i18next";
import Spinner from "@/components/LoadingSpinner";

export default function RoleDetails({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tRole } = useTranslation("approle");
	const { t: tCommon } = useTranslation("common");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [data, setData] = useState<IRole>();
	const [hydrated, setHydrated] = useState(false);
	const roleService: RoleService = useMemo(
		() => new RoleService(),
		[]
	);

	if (setAccountInfo) {
		roleService.injectSetAccountInfo(setAccountInfo);
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
				setData((await roleService.getAsync(id)).data!);
			};
			fetchData();
		}
	}, [
		hydrated,
		accountInfo,
		router,
		id,
		roleService,
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
					{tRole("AppRoleName")}
				</h4>
				<hr className="border-gray-300" />
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tRole("AppRoleName")}
						</dt>
						<dd className="w-2/3 text-gray-900">
							{data.name}
						</dd>
					</div>
				</dl>
			</div>
			<div>
				<Link
					href="/roles"
					className="text-blue-600 hover:underline font-medium"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
