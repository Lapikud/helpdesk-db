"use client";
import LanguageSwitcher from "@/components/LanguageSwitcher";
import { AccountContext } from "../context/AccountContext";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useContext, useState, useRef, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { AccountService } from "@/services/AccountService";

export default function Header() {
	const { t: tLayout } = useTranslation("_layout");
	const { t: tCommon } = useTranslation("common");
	const { t: tLoginPartial } = useTranslation("_loginpartial");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [isUserDropdownOpen, setIsUserDropdownOpen] = useState(false);
	const userDropdownRef = useRef<HTMLDivElement>(null);

	const [isAdminDropdownOpen, setIsAdminDropdownOpen] = useState(false);
	const adminDropdownRef = useRef<HTMLDivElement>(null);

	const isAdmin = accountInfo?.roles?.includes("admins");

	useEffect(() => {
		function handleClickOutside(event: MouseEvent) {
			if (
				userDropdownRef.current &&
				!userDropdownRef.current.contains(event.target as Node)
			) {
				setIsUserDropdownOpen(false);
			}
			if (
				adminDropdownRef.current &&
				!adminDropdownRef.current.contains(event.target as Node)
			) {
				setIsAdminDropdownOpen(false);
			}
		}
		document.addEventListener("mousedown", handleClickOutside);
		return () => {
			document.removeEventListener("mousedown", handleClickOutside);
		};
	}, [userDropdownRef, adminDropdownRef]);

	return (
		<nav className="bg-white border-b shadow mb-3">
			<div className="container mx-auto px-4 py-3 flex items-center justify-between">
				{/* Left side: Brand + Main nav */}
				<div className="flex items-center space-x-6">
					<Link
						href="/"
						className="text-xl font-semibold text-gray-800"
					>
						{tLayout("AppName")}
					</Link>

					<Link
						href="/"
						className="text-gray-700 hover:text-blue-500"
					>
						{tCommon("Home")}
					</Link>

					{/* User Dropdown */}
					{accountInfo?.jwt && (
						<div className="relative" ref={userDropdownRef}>
							<button
								onClick={() =>
									setIsUserDropdownOpen(!isUserDropdownOpen)
								}
								className="text-gray-700 hover:text-blue-500 flex items-center gap-1"
							>
								{tLayout("User")}
								<svg
									className="w-4 h-4"
									fill="none"
									stroke="currentColor"
									viewBox="0 0 24 24"
								>
									<path
										strokeLinecap="round"
										strokeLinejoin="round"
										strokeWidth={2}
										d="M19 9l-7 7-7-7"
									/>
								</svg>
							</button>
							{isUserDropdownOpen && (
								<div className="absolute mt-2 w-48 bg-white border border-gray-200 rounded shadow z-50">
									<Link
										href="/assetReservations"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsUserDropdownOpen(false)
										}
									>
										{tLayout("AssetReservations")}
									</Link>
									<Link
										href="/categories"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsUserDropdownOpen(false)
										}
									>
										{tLayout("Categories")}
									</Link>
									<Link
										href="/owners"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsUserDropdownOpen(false)
										}
									>
										{tLayout("Owners")}
									</Link>
									<Link
										href="/removedAssets"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsUserDropdownOpen(false)
										}
									>
										{tLayout("RemovedAssets")}
									</Link>
								</div>
							)}
						</div>
					)}
					{/* Admin Dropdown */}
					{accountInfo?.jwt && isAdmin && (
						<div className="relative" ref={adminDropdownRef}>
							<button
								onClick={() =>
									setIsAdminDropdownOpen(!isAdminDropdownOpen)
								}
								className="text-gray-700 hover:text-blue-500 flex items-center gap-1"
							>
								{tLayout("Admin")}
								<svg
									className="w-4 h-4"
									fill="none"
									stroke="currentColor"
									viewBox="0 0 24 24"
								>
									<path
										strokeLinecap="round"
										strokeLinejoin="round"
										strokeWidth={2}
										d="M19 9l-7 7-7-7"
									/>
								</svg>
							</button>
							{isAdminDropdownOpen && (
								<div className="absolute mt-2 w-48 bg-white border border-gray-200 rounded shadow z-50">
									<Link
										href="/dbassets"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("Assets")}
									</Link>
									<Link
										href="/assetReservations"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsUserDropdownOpen(false)
										}
									>
										{tLayout("AssetReservations")}
									</Link>
									<Link
										href="/categories"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("Categories")}
									</Link>
									<Link
										href="/categoryAssets"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("CategoryAssets")}
									</Link>
									<Link
										href="/cupboards"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("Cupboards")}
									</Link>
									<Link
										href="/cupboardsInRooms"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("CupboardsInRooms")}
									</Link>
									<Link
										href="/locations"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("Locations")}
									</Link>
									<Link
										href="/locationAssets"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("LocationAssets")}
									</Link>
									<Link
										href="/locationsInCupboards"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("LocationInCupboards")}
									</Link>
									<Link
										href="/owners"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("Owners")}
									</Link>
									<Link
										href="/rooms"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("Rooms")}
									</Link>
									<Link
										href="/removedAssets"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("RemovedAssets")}
									</Link>
									<Link
										href="/userAssets"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("UserAssets")}
									</Link>
									<hr className="my-1 border-gray-200" />
									<Link
										href="/users"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("Users")}
									</Link>
									<Link
										href="/roles"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("Roles")}
									</Link>
									<Link
										href="/userRoles"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("UserRoles")}
									</Link>
									<Link
										href="/refreshTokens"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("RefreshTokens")}
									</Link>
									<hr className="my-1 border-gray-200" />
									<Link
										href="/userManagement"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("UserManagement")}
									</Link>
								</div>
							)}
						</div>
					)}
				</div>
				<div className="flex items-center space-x-6">
					<LanguageSwitcher />
					{accountInfo?.jwt && (
						<div className="flex items-center space-x-4">
							<span>
								{tLoginPartial("Greeting")}, {accountInfo.name}
							</span>
							<a
								className="text-gray-700 hover:text-blue-500"
								href="#"
								onClick={async () => {
									const refreshToken = accountInfo?.refreshToken;
									if (refreshToken) {
										const accountService = new AccountService();
										await accountService.logoutAsync(refreshToken);
									}
									setAccountInfo!({});
									router.push("/login");
								}}
							>
								{tCommon("Logout")}
							</a>
						</div>
					)}
					{!accountInfo?.jwt && (
						<Link
							href="/login"
							className="text-gray-700 hover:text-blue-500"
						>
							{tCommon("LoginLink")}
						</Link>
					)}
				</div>
			</div>

			{/* Original commented-out Bootstrap markup preserved below */}
			{/*
			<li className="nav-item dropdown">
				<a className="nav-link text-dark dropdown-toggle" href="javascript:{}" id="adminDropdown"
					role="button" data-bs-toggle="dropdown" aria-expanded="false">Admin</a>
				<div className="dropdown-menu" aria-labelledby="adminDropdown">

					<a className="dropdown-item text-dark" href="/admin/Assets">Assets</a>
					<a className="dropdown-item text-dark" href="/admin/Categories">Categories</a>
					<a className="dropdown-item text-dark" href="/admin/CategoryAssets">Category assets</a>
					<a className="dropdown-item text-dark" href="/admin/Cupboards">Cupboards</a>
					<a className="dropdown-item text-dark" href="/admin/CupboardsInRooms">Cupboards in rooms</a>
					<a className="dropdown-item text-dark" href="/admin/Locations">Locations</a>
					<a className="dropdown-item text-dark" href="/admin/LocationAssets">Location assets</a>
					<a className="dropdown-item text-dark" href="/admin/LocationsInCupboards">Location in cupboards</a>
					<a className="dropdown-item text-dark" href="/admin/Owners">Owners</a>
					<a className="dropdown-item text-dark" href="/admin/OwnerAssets">Owner assets</a>
					<a className="dropdown-item text-dark" href="/admin/Rooms">Rooms</a>
					<a className="dropdown-item text-dark" href="/admin/RemovedAssets">Removed assets</a>
					<a className="dropdown-item text-dark" href="/admin/UserAssets">User assets</a>

					<hr className="dropdown-divider">

					<a className="dropdown-item text-dark" href="/admin/Users">Users</a>
					<a className="dropdown-item text-dark" href="/admin/Roles">Roles</a>
					<a className="dropdown-item text-dark" href="/admin/UserRoles">User roles</a>
					<a className="dropdown-item text-dark" href="/admin/RefreshTokens">Refresh tokens</a>

					<hr className="dropdown-divider">

					<a className="dropdown-item text-dark" href="/admin/UserManagement">User management</a>
				</div>
			</li>
			*/}
		</nav>
	);
}
