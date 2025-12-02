function setScrollPosition(key, position) {
    sessionStorage.setItem(key, position.toString());
}

function getScrollPosition(key) {
    const position = sessionStorage.getItem(key);
    if (position) {
        window.scrollTo(0, parseInt(position, 10));
    }
}

// New helper function to safely get scrollY property
window.getScrollY = function () {
    return window.scrollY;
};