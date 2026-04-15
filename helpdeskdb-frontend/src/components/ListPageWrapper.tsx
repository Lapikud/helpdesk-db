interface ListPageWrapperProps {
	title: string;
	createButton?: React.ReactNode;
	children: React.ReactNode;
}

export default function ListPageWrapper({
	title,
	createButton,
	children,
}: ListPageWrapperProps) {
	return (
		<div className="min-h-screen bg-[#efefef] -mx-3 sm:-mx-4 px-6 sm:px-14 py-8 text-left">
			<div className="flex items-center justify-between mb-6">
				<h1 className="text-3xl font-bold text-[#424242]">{title}</h1>
				{createButton}
			</div>
			{children}
		</div>
	);
}
