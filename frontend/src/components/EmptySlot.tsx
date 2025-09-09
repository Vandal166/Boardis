import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';

function EmptySlot({ id }: { id: string })
{
    // Register as sortable; do not spread listeners/attributes => not draggable
    const { setNodeRef, transform, transition } = useSortable({ id });

    const style = {
        transform: CSS.Transform.toString(transform),
        transition,
    };

    return (
        <div
            ref={setNodeRef}
            style={style}
            className={`w-[300px] min-h-[220px] border-2 border-dashed rounded-lg transition-colors
            border-gray-400 bg-transparent`}
        />
    );
}

export default EmptySlot;