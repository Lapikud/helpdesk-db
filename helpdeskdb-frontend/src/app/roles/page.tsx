"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { RoleService } from "@/services/RoleService";
import { useRouter } from "next/navigation";

import Link from "next/link";
import { useContext, useEffect, useMemo, useState } from "react";
import { IRole } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";

export default function Roles() {
	const { t: tRole } = useTranslation("approle");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const roleService: RoleService = useMemo(() => new RoleService(), []);
	if (setAccountInfo) {
		roleService.injectSetAccountInfo(setAccountInfo);
	}
	const router = useRouter();
	const [data, setData] = useState<IRole[]>([]);
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
				const result = await roleService.getAllAsync();
				if (result.errors) {
					console.log(result.errors);
					return;
				}
				setData(result.data!);
			} catch (error) {
				console.error("Error fetching data:", error);
			}
		};

		fetchData();
	}, [hydrated, accountInfo, router, roleService, isAdmin]);

	if (!hydrated) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-4">Roles</h1>

			{(isAdmin) && (
				<p className="mb-4">
					<Link
						href="/roles/create"
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
								Name{/* {tRole("RoleName")} */}
							</th>
							{isAdmin && (
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
									{item.name}
								</td>
								<td className="px-6 py-4 border-b text-blue-600 space-x-2">
									<Link
										href={`/roles/edit/${item.id}`}
										className="hover:underline"
									>
										{tCommon("EditLink")}
									</Link>
									<span className="text-gray-400">|</span>
									<Link
										href={`/roles/details/${item.id}`}
										className="hover:underline"
									>
										{tCommon("DetailsLink")}
									</Link>
									<span className="text-gray-400">|</span>
									<Link
										href={`/roles/delete/${item.id}`}
										className="hover:underline"
									>
										{tCommon("DeleteLink")}
									</Link>
								</td>
							</tr>
						))}
					</tbody>
				</table>
			</div>
		</>
	);
}
