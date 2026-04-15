const gridMap: Record<number, string> = {
	1: "grid-cols-1",
	2: "grid-cols-2",
	3: "grid-cols-3",
	4: "grid-cols-4",
	5: "grid-cols-5",
	6: "grid-cols-6",
	7: "grid-cols-7",
	8: "grid-cols-8",
};

export interface DataTableRow {
	id: string;
	cells: React.ReactNode[];
}

interface DataTableProps {
	columns: string[];
	rows: DataTableRow[];
	minWidth?: string;
	emptyMessage?: string;
}

export default function DataTable({
	columns,
	rows,
	minWidth = "min-w-[400px]",
	emptyMessage = "No data to display.",
}: DataTableProps) {
	const gridCols = gridMap[columns.length] ?? "grid-cols-4";

	return (
		<div className="bg-white rounded-3xl shadow-sm p-4">
			<div className="overflow-x-auto">
				<div
					className={`${minWidth} bg-[#efefef] rounded-2xl overflow-hidden`}
				>
					{/* Header */}
					<div
						className={`bg-[#424242] text-white text-sm rounded-full mx-4 mt-4 px-6 py-3 grid gap-4 ${gridCols}`}
					>
						{columns.map((col) => (
							<div key={col} className="text-center">
								{col}
							</div>
						))}
					</div>

					{/* Rows */}
					<div className="flex flex-col gap-1 p-4 max-h-[600px] overflow-y-auto">
						{rows.length === 0 ? (
							<div className="text-center py-8 text-gray-500">
								{emptyMessage}
							</div>
						) : (
							rows.map((row) => (
								<div
									key={row.id}
									className={`bg-white rounded-2xl px-6 py-3 grid gap-4 items-center text-sm text-black ${gridCols}`}
								>
									{row.cells.map((cell, i) => (
										<div
											key={i}
											className="p-2 text-center truncate hover:whitespace-normal"
										>
											{cell}
										</div>
									))}
								</div>
							))
						)}
					</div>
				</div>
			</div>
		</div>
	);
}
