<template>
    <div class="xss-demo">
        <h1>XSS Demonstration</h1>
        
        <div class="input-form">
            <form @submit.prevent="handleSubmit">
                <input 
                    type="text"
                    v-model="inputString" 
                    placeholder="Be careful with your input!"
                    autocomplete="off"
                />
            </form>
        </div>
        
        <div class="results" v-if="submittedInputs.length > 0">
            <h2>Submitted Inputs</h2>
            <table>
                <thead>
                    <tr>
                        <th>#</th>
                        <th>Input</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="(input, index) in submittedInputs" :key="index">
                        <td>{{ index + 1 }}</td>
                        <td>{{ input }}</td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';

const inputString = ref<string>('');
const submittedInputs = ref<string[]>(["Initial input"]);

const handleSubmit = () => {
    if (!inputString.value) { return; }

    if (inputString.value.trim()) {
        const xssString = `alert(\`Inserted ${inputString.value}\`)`;
        submittedInputs.value.push(xssString);
        
        eval(xssString);
        inputString.value = '';
    }
}
</script>

<style scoped lang="css">
.xss-demo {
    max-width: 800px;
    margin: 0 auto;
    padding: 20px;
}

input {
    width: 100%;
    padding: 8px;
    margin-bottom: 20px;
    border: 1px solid red;
    border-radius: 4px;
}

table {
    width: 100%;
    border-collapse: collapse;
}

th, td {
    padding: 8px;
    text-align: left;
    border-bottom: 1px solid #ddd;
}
</style>