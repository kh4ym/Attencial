window.authStorage = {
    setToken: function (token) {
        localStorage.setItem("jwt_token", token);
    },
    getToken: function () {
        return localStorage.getItem("jwt_token");
    },
    removeToken: function () {
        localStorage.removeItem("jwt_token");
    }
};

window.dashboardPrefetch = {
    key: "attencial_dashboard_prefetch",

    start: function (token, role, email) {
        if (!token || token === "null" || token === "undefined") {
            return;
        }

        const headers = {
            Authorization: `Bearer ${token}`
        };

        const snapshot = {
            role: role || "",
            email: email || "",
            timestamp: Date.now()
        };

        if ((role || "").toLowerCase() === "professor") {
            Promise.all([
                fetch("api/attendance/professor/courses", { headers }).then(r => r.ok ? r.json() : null),
                fetch("api/courses/enrollment-requests/pending", { headers }).then(r => r.ok ? r.json() : null),
                fetch("api/professor/appeals/pending", { headers }).then(r => r.ok ? r.json() : null)
            ]).then(async ([coursesRes, enrollmentsRes, appealsRes]) => {
                const courseItems = Array.isArray(coursesRes?.data) ? coursesRes.data : [];
                const courseIds = courseItems
                    .map(item => item?.id)
                    .filter(id => Number.isInteger(id));

                const today = new Date();
                today.setHours(0, 0, 0, 0);
                const tomorrow = new Date(today);
                tomorrow.setDate(tomorrow.getDate() + 1);

                const sessionCounts = await Promise.all(courseIds.map(async (courseId) => {
                    try {
                        const response = await fetch(`api/professor/courses/${courseId}/sessions`, { headers });
                        if (!response.ok) {
                            return 0;
                        }
                        const json = await response.json();
                        const sessions = Array.isArray(json?.data) ? json.data : [];
                        return sessions.filter(session => {
                            const start = new Date(session.startTime);
                            return start >= today && start < tomorrow;
                        }).length;
                    } catch {
                        return 0;
                    }
                }));

                snapshot.professor = {
                    activeCourses: courseIds.length,
                    todaySessions: sessionCounts.reduce((sum, count) => sum + count, 0),
                    pendingEnrollments: Array.isArray(enrollmentsRes?.data) ? enrollmentsRes.data.length : 0,
                    pendingAppeals: Array.isArray(appealsRes?.data) ? appealsRes.data.length : 0
                };

                sessionStorage.setItem(this.key, JSON.stringify(snapshot));
            }).catch(() => { });
            return;
        }

        Promise.all([
            fetch("api/enrollment/status", { headers }).then(r => r.ok ? r.json() : null),
            fetch("api/students/me/attendance", { headers }).then(r => r.ok ? r.json() : null)
        ]).then(([enrollmentRes, attendanceRes]) => {
            const enrollmentData = enrollmentRes?.data || {};
            snapshot.student = {
                isEnrolled: !!enrollmentData.isEnrolled,
                enrollmentStatus: enrollmentData.isEnrolled ? "Active" : "Pending",
                summary: attendanceRes?.data || null
            };

            sessionStorage.setItem(this.key, JSON.stringify(snapshot));
        }).catch(() => { });
    },

    peek: function () {
        return sessionStorage.getItem(this.key) || "";
    },

    clear: function () {
        sessionStorage.removeItem(this.key);
    }
};
