"use client";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { RoleService } from "@/services/RoleService";
import { IRole } from "@/types/domain/DomainTypes";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";

export default function RoleDelete({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tRole } = useTranslation("approle");
	const { t: tCommon } = useTranslation("common");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [errorMessage, setErrorMessage] = useState("");
	const [data, setData] = useState<IRole>();
	const roleService: RoleService = useMemo(() => new RoleService(), []);

	if (setAccountInfo) {
		roleService.injectSetAccountInfo(setAccountInfo);
	}

	const isAdmin = accountInfo?.roles?.includes("admins");
	const isMember = accountInfo?.roles?.includes("members");

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;

		if (!accountInfo?.jwt) {
			router.push("/login");
		} else if (!isAdmin || !isMember) {
			router.push("/roles");
		} else {
			const fetchData = async () => {
				setData((await roleService.getAsync(id)).data!);
			};
			fetchData();
		}
	}, [hydrated, accountInfo, router, id, isAdmin, isMember, roleService]);

	const deleteConfirmed = async () => {
		try {
			const result = await roleService.deleteAsync(id);

			console.log("delete result", result);

			if (result.errors && result.errors.length > 0) {
				setErrorMessage(
					result.statusCode + " - " + result.errors.join(", ")
				);
				return;
			} else {
				setErrorMessage("");
				router.push("/roles");
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
					{tRole("AppRoleName")}
				</h4>
				<hr className="border-gray-300" />
				{errorMessage.length > 0 && (
					<p className="text-red-600">{errorMessage}</p>
				)}
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
						href="/roles"
						className="text-blue-600 hover:underline font-medium"
					>
						{tCommon("BackToListLink")}
					</Link>
				</div>
			</div>
		</>
	);
}
