import { useTranslation } from "react-i18next";

const variantClasses = {
	solid: "bg-[#ff9800] hover:bg-[#f0941d] text-white",
	outline: "border border-[#ff9800] text-[#f0941d] hover:bg-orange-50",
} as const;

interface CreateButtonProps {
	onClick: () => void;
	/** Defaults to the common CreateNewLink label. */
	label?: string;
	variant?: keyof typeof variantClasses;
}

/**
 * The orange pill button passed into ListPageWrapper's `createButton` slot.
 * "outline" is the secondary style for pages with two create actions.
 */
export default function CreateButton({
	onClick,
	label,
	variant = "solid",
}: CreateButtonProps) {
	const { t: tCommon } = useTranslation("common");

	return (
		<button
			type="button"
			onClick={onClick}
			className={`${variantClasses[variant]} font-medium px-6 py-3 rounded-full text-sm whitespace-nowrap transition-colors duration-150`}
		>
			{label ?? tCommon("CreateNewLink")}
		</button>
	);
}
