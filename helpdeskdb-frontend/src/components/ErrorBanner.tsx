import { useTranslation } from "react-i18next";

interface ErrorBannerProps {
	/** Renders nothing when falsy, so callers can pass the raw query error. */
	error: Error | string | null | undefined;
	/**
	 * "page" — the list-page LoadFailed banner (prefixes the translated
	 * failure message); "dialog" — the modal-level error box, message only.
	 */
	variant?: "page" | "dialog";
}

export default function ErrorBanner({
	error,
	variant = "page",
}: ErrorBannerProps) {
	const { t: tCommon } = useTranslation("common");

	if (!error) return null;
	const message = typeof error === "string" ? error : error.message;

	if (variant === "dialog") {
		return (
			<div className="mb-4 p-3 bg-red-100 text-red-700 border border-red-400 rounded">
				{message}
			</div>
		);
	}

	return (
		<div className="mb-4 rounded-lg bg-red-100 border border-red-300 text-red-700 px-4 py-3">
			{tCommon("LoadFailed")}
			{message ? `: ${message}` : ""}
		</div>
	);
}
