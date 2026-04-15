import Link from "next/link";

const btnBase =
	"text-sm font-medium py-2 px-4 rounded-xl whitespace-nowrap transition-colors duration-150 text-center block w-full";

/** Wraps action buttons in the actions cell */
export function ActionCell({ children }: { children: React.ReactNode }) {
	return (
		<div className="flex justify-center">
			<div className="flex flex-col gap-1.5 w-24">{children}</div>
		</div>
	);
}

interface ActionButtonProps {
	href?: string;
	onClick?: () => void;
	label: string;
	className: string;
}

function ActionButton({ href, onClick, label, className }: ActionButtonProps) {
	if (href) {
		return (
			<Link href={href} className={`${btnBase} ${className}`}>
				{label}
			</Link>
		);
	}
	return (
		<button onClick={onClick} className={`${btnBase} ${className}`}>
			{label}
		</button>
	);
}

export function EditButton({
	href,
	onClick,
	label = "Edit",
}: {
	href?: string;
	onClick?: () => void;
	label?: string;
}) {
	return (
		<ActionButton
			href={href}
			onClick={onClick}
			label={label}
			className="bg-[#e3f2fd] hover:bg-blue-100 text-[#50b3f1]"
		/>
	);
}

export function DeleteButton({
	href,
	onClick,
	label = "Delete",
}: {
	href?: string;
	onClick?: () => void;
	label?: string;
}) {
	return (
		<ActionButton
			href={href}
			onClick={onClick}
			label={label}
			className="bg-[#ffebee] hover:bg-red-100 text-[#ea6e6c]"
		/>
	);
}
