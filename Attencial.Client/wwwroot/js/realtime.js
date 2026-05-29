window.supabaseRealtime = {
    client: null,
    subscription: null,
    dotNetHelper: null,

    initialize: function (url, anonKey, dotnetRef, courseId, sessionId) {
        this.dotNetHelper = dotnetRef;

        // If credentials are placeholder or default, run in simulation mode
        var isPlaceholder = !url || url.includes("placeholder") || !anonKey || anonKey.includes("placeholder") || anonKey.length < 20;

        if (isPlaceholder) {
            console.log("Supabase Realtime parameters missing or placeholder. Running in simulation mode.");
            this.startSimulation(courseId, sessionId);
            return "simulated";
        }

        try {
            // Initialize Supabase Client
            this.client = supabase.createClient(url, anonKey);

            // Subscribe to AttendanceRecords table inserts for this session
            this.subscription = this.client
                .channel('public-AttendanceRecords')
                .on(
                    'postgres_changes',
                    {
                        event: 'INSERT',
                        schema: 'public',
                        table: 'AttendanceRecords',
                        filter: 'SessionId=eq.' + sessionId
                    },
                    (payload) => {
                        console.log('Realtime Attendance Insert Payload:', payload);
                        if (this.dotNetHelper && payload.new) {
                            this.dotNetHelper.invokeMethodAsync(
                                'OnAttendanceMarkedRealtime',
                                payload.new.StudentId,
                                payload.new.Confidence || 100.0,
                                payload.new.MarkedAt || new Date().toISOString()
                            );
                        }
                    }
                )
                .subscribe();

            console.log("Successfully subscribed to Supabase Realtime for SessionId: " + sessionId);
            return "connected";
        } catch (e) {
            console.error("Failed to initialize Supabase Realtime client:", e);
            this.startSimulation(courseId, sessionId);
            return "simulated";
        }
    },

    disconnect: function () {
        if (this.subscription && this.client) {
            this.client.removeChannel(this.subscription);
            this.subscription = null;
        }
        this.client = null;
        this.dotNetHelper = null;
        this.stopSimulation();
    },

    // Local simulation fallback
    simTimer: null,
    startSimulation: function (courseId, sessionId) {
        this.stopSimulation();
        console.log("Started local simulation for Course: " + courseId + ", Session: " + sessionId);
        
        var self = this;
        var simulateNext = function() {
            var delay = Math.floor(Math.random() * 8000) + 7000; // 7-15s
            self.simTimer = setTimeout(async function() {
                if (self.dotNetHelper) {
	                    try {
	                        var token = window.authStorage ? window.authStorage.getToken() : null;
	                        var response = await fetch('/api/attendance/sessions/' + sessionId + '/simulate-scan', {
	                            method: 'POST',
	                            headers: {
	                                'Content-Type': 'application/json',
	                                ...(token ? { 'Authorization': 'Bearer ' + token } : {})
	                            }
	                        });
                        var result = await response.json();
                        if (result.success && result.data) {
                            console.log("Simulated Scan Event:", result.data);
                            self.dotNetHelper.invokeMethodAsync(
                                'OnAttendanceMarkedSimulated', 
                                result.data.studentId, 
                                result.data.studentName, 
                                result.data.rollNumber, 
                                result.data.confidence, 
                                result.data.markedAt
                            );
                        }
                    } catch (err) {
                        console.error("Simulation request failed", err);
                    }
                }
                simulateNext();
            }, delay);
        };
        simulateNext();
    },

    stopSimulation: function () {
        if (this.simTimer) {
            clearTimeout(this.simTimer);
            this.simTimer = null;
        }
    }
};
