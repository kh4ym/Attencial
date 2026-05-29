window.supabaseRealtime = {
    client: null,
    subscription: null,
    dotNetHelper: null,

    initialize: function (url, anonKey, dotnetRef, courseId, sessionId) {
        this.dotNetHelper = dotnetRef;

        var isMissing = !url || url.includes("placeholder") || !anonKey || anonKey.includes("placeholder") || anonKey.length < 20;

        if (isMissing) {
            console.warn("Supabase Realtime parameters are missing or placeholder. Real-time subscription disabled.");
            return "disabled";
        }

        try {
            // Initialize Supabase Client (disable session persistence as we only use it for public realtime)
            this.client = supabase.createClient(url, anonKey, {
                auth: {
                    persistSession: false
                }
            });

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
                        console.log('Real-time Attendance Insert Payload:', payload);
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
            return "error";
        }
    },

    disconnect: function () {
        if (this.subscription && this.client) {
            this.client.removeChannel(this.subscription);
            this.subscription = null;
        }
        this.client = null;
        this.dotNetHelper = null;
    }
};
