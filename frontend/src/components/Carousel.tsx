import { useState } from 'react';
import placeholderImg from '../assets/placeholder.jpg';
import placeholderImg2 from '../assets/placeholder2.jpg';

function Carousel()
{
    const [carouselIndex, setCarouselIndex] = useState(0);
    const carouselImages = [placeholderImg, placeholderImg2];

    const nextSlide = () => setCarouselIndex((prev) => (prev + 1) % carouselImages.length);
    const prevSlide = () => setCarouselIndex((prev) => (prev - 1 + carouselImages.length) % carouselImages.length);

    return (
        <div className="md:w-1/2 relative flex justify-center">
            <div className="relative h-96 overflow-hidden rounded-lg shadow-2xl flex items-center justify-center">
                <img
                    src={carouselImages[carouselIndex]}
                    alt={`Showcase ${carouselIndex + 1}`}
                    className="w-full h-full object-cover"
                />
            </div>
            <button
                onClick={prevSlide}
                className="absolute left-2 top-1/2 transform -translate-y-1/2 text-white p-2 rounded-full opacity-75 hover:opacity-100 transition"
            >
                &lt;
            </button>
            <button
                onClick={nextSlide}
                className="absolute right-2 top-1/2 transform -translate-y-1/2 text-white p-2 rounded-full opacity-75 hover:opacity-100 transition"
            >
                &gt;
            </button>
        </div>
    );
}

export default Carousel;