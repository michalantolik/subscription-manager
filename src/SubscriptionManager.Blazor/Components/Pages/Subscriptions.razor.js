const scrollThreshold = 2;
const rails = new WeakMap();

export function initialize(element, dotNetReference) {
    dispose(element);

    const state = {
        dotNetReference,
        animationFrame: 0,
        resizeObserver: null
    };

    const scheduleUpdate = () => {
        if (state.animationFrame !== 0) {
            cancelAnimationFrame(state.animationFrame);
        }

        state.animationFrame = requestAnimationFrame(() => {
            state.animationFrame = 0;
            publishState(element, state.dotNetReference);
        });
    };

    state.scheduleUpdate = scheduleUpdate;
    state.resizeObserver = new ResizeObserver(scheduleUpdate);

    element.addEventListener(
        "scroll",
        scheduleUpdate,
        { passive: true });

    state.resizeObserver.observe(element);
    rails.set(element, state);

    scheduleUpdate();
}

export function scroll(element, direction) {
    const distance = Math.max(
        180,
        Math.round(element.clientWidth * 0.72));

    element.scrollBy({
        left: direction * distance,
        behavior: scrollBehavior()
    });
}

export function refresh(
    element,
    revealSelected = false) {
    const state = rails.get(element);

    if (!state) {
        return;
    }

    if (revealSelected) {
        const selected =
            element.querySelector(
                '[aria-current="true"]');

        selected?.scrollIntoView({
            behavior: scrollBehavior(),
            block: "nearest",
            inline: "nearest"
        });
    }

    state.scheduleUpdate();
}

export function dispose(element) {
    const state = rails.get(element);

    if (!state) {
        return;
    }

    element.removeEventListener(
        "scroll",
        state.scheduleUpdate);

    state.resizeObserver?.disconnect();

    if (state.animationFrame !== 0) {
        cancelAnimationFrame(state.animationFrame);
    }

    rails.delete(element);
}

function publishState(element, dotNetReference) {
    const maximumScrollLeft = Math.max(
        0,
        element.scrollWidth - element.clientWidth);

    const canScrollLeft =
        element.scrollLeft > scrollThreshold;

    const canScrollRight =
        element.scrollLeft <
        maximumScrollLeft - scrollThreshold;

    dotNetReference.invokeMethodAsync(
        "UpdateCategoryScrollState",
        canScrollLeft,
        canScrollRight);
}

function scrollBehavior() {
    return window
        .matchMedia(
            "(prefers-reduced-motion: reduce)")
        .matches
        ? "auto"
        : "smooth";
}
