<template>
  <button @click="openreport">打开报表</button>
</template>

<script>
import http from "@/../src/api/http.js";
export default {

methods:{
    name:'fastreport',
    openreport(){
        window.open(http.ipAddress+'api/fastreporttest/index' )
    }
}



}
</script>

<style>

</style>