(function (global) {
    'use strict';

    function seatKey(r, c) {
        return r + ',' + c;
    }

    function getSeatMap(seats) {
        const map = {};
        for (const s of seats) map[seatKey(s.row, s.column)] = s;
        return map;
    }

    function sortSeats(seats) {
        return [...seats].sort((a, b) => a.row - b.row || a.column - b.column);
    }

    function renumberSeats(seats) {
        const sorted = sortSeats(seats);
        sorted.forEach((s, i) => s.seatNumber = i + 1);
        return sorted;
    }

    function filterToBounds(seats, rows, cols) {
        return seats.filter(s => s.row < rows && s.column < cols);
    }

    function computeInvalidPositions(seats, capacity) {
        const invalid = new Set();
        if (capacity == null || capacity < 0) return invalid;
        if (seats.length <= capacity) return invalid;

        const saved = [];
        const unsaved = [];
        for (const s of seats) {
            if (s.id) saved.push(s);
            else unsaved.push(s);
        }
        unsaved.sort((a, b) => (a.insertedAt || 0) - (b.insertedAt || 0));

        const slotsForUnsaved = Math.max(0, capacity - saved.length);
        for (let i = slotsForUnsaved; i < unsaved.length; i++) {
            invalid.add(seatKey(unsaved[i].row, unsaved[i].column));
        }
        return invalid;
    }

    const api = {
        seatKey,
        getSeatMap,
        sortSeats,
        renumberSeats,
        filterToBounds,
        computeInvalidPositions
    };

    if (typeof module !== 'undefined' && module.exports) {
        module.exports = api;
    } else {
        global.SeatLayout = api;
    }
})(typeof window !== 'undefined' ? window : globalThis);
