"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { UserService } from "@/services/UserService";
import { useRouter } from "next/navigation";

import Link from "next/link";
import { useContext, useEffect, useMemo, useState } from "react";
import { IUser } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";

export default function Users() {
	// const { t: tUser } = useTranslation("user");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const userService: UserService = useMemo(() => new UserService(), []);
	if (setAccountInfo) {
		userService.injectSetAccountInfo(setAccountInfo);
	}
	const router = useRouter();
	const [data, setData] = useState<IUser[]>([]);
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
				const result = await userService.getAllAsync();
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
	}, [hydrated, accountInfo, router, userService, isAdmin]);

	if (!hydrated) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-4">Users</h1>

			<div className="w-full max-w-7xl overflow-x-auto shadow rounded-lg">
				<table className="w-full table-auto bg-white border border-gray-200 text-left">
					<thead className="bg-gray-100">
						<tr>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								Username{/* {tUser("UserName")} */}
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
									{item.username}
								</td>
								<td className="px-6 py-4 border-b text-blue-600 space-x-2">
									<Link
										href={`/users/details/${item.id}`}
										className="hover:underline"
									>
										{tCommon("DetailsLink")}
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
