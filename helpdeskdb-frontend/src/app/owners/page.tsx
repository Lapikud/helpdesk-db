"use client";

import { AccountContext } from "@/context/AccountContext";
import { OwnerService } from "@/services/OwnerService";
import { useRouter } from "next/navigation";

import Link from "next/link";
import { useContext, useEffect, useMemo, useState } from "react";
import { IOwner } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";

export default function Owners() {
	const { t: tOwner } = useTranslation("owner");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const ownerService: OwnerService = useMemo(() => new OwnerService(), []);
	if (setAccountInfo) {
		ownerService.injectSetAccountInfo(setAccountInfo);
	}
	const router = useRouter();
	const [data, setData] = useState<IOwner[]>([]);
	const [hydrated, setHydrated] = useState(false);

	const isAdmin = accountInfo?.roles?.includes("admins");
	const isMember = accountInfo?.roles?.includes("members");

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;

		if (!accountInfo?.jwt) {
			router.push("/login");
		}

		const fetchData = async () => {
			try {
				const result = await ownerService.getAllAsync();
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
	}, [hydrated, accountInfo, router, ownerService]);

	if (!hydrated) {
		return <Spinner className="h-64" />;
	}
	return (
		<>
			<h1 className="text-3xl font-semibold mb-4">{tOwner("Owners")}</h1>

			{(isAdmin) && (
				<p className="mb-4">
					<Link
						href="/owners/create"
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
								{tOwner("OwnerName")}
							</th>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								{tCommon("Comment")}
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
									{item.ownerName}
								</td>
								<td className="px-6 py-4 border-b">
									{item.comment}
								</td>
								{(isAdmin) && (
										<td className="px-6 py-4 border-b text-blue-600 space-x-2">
											<Link
												href={`/owners/edit/${item.id}`}
												className="hover:underline"
											>
												{tCommon("EditLink")}
											</Link>
											<span className="text-gray-400">
												|
											</span>
											<Link
												href={`/owners/details/${item.id}`}
												className="hover:underline"
											>
												{tCommon("DetailsLink")}
											</Link>
											<span className="text-gray-400">
												|
											</span>
											<Link
												href={`/owners/delete/${item.id}`}
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
