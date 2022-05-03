<script>
import { fetchEventSource } from '@microsoft/fetch-event-source';
class RetriableError extends Error { }
class FatalError extends Error { }

export default {
	created() {
		this.source = fetchEventSource('http://localhost:5024/gql/streams', {
			headers: {
				'Content-Type': 'application/json'
			},
			body: '{"query":"subscription testSubscription {\\n\\tfasterMessageAdded2\\n}"}',
			method: 'POST',
			onmessage: (e) => {
				if(e.event === 'error')
					throw new FatalError(e.data);

				this.data = e.data;
			},
			onclose: (e) => {
				// trigger an error to make it retry
				throw new RetriableError();
			},
			onerror: (err) => {
				if (err instanceof FatalError) {
					throw err; // rethrow to stop the operation
				}
				// do nothing to automatically retry
				return 1000; // wait 1 second before retry
			}
		});
	},
	unmounted() {
		// TODO: what is correct way to close, documentation lacking
		this.source.close();
	},
	data() {
		return {
			data: 'No data'
		}
	}
}
</script>

<template>
	Latest data value: {{ data }}
</template>
